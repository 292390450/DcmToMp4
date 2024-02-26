using FFmpeg.AutoGen;

namespace DcmExport
{
    public unsafe class Mp4Export
    {
        private int outVStreamIndex;
        private int inVStreamIndex;
        public Mp4Export(string h264, string outPath,int rate)
        {
            var infmt_ctxP = ffmpeg.avformat_alloc_context();
            ffmpeg.avformat_open_input(&infmt_ctxP, h264, null, null).ThrowExceptionIfError();
            


            ffmpeg.avformat_find_stream_info(infmt_ctxP, null).ThrowExceptionIfError();
            //查找视频流在文件中的位置
            for (var i = 0; i < infmt_ctxP->nb_streams; i++)
            {
                if (infmt_ctxP->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    inVStreamIndex = (int)i;
                    break;
                }
            }
            AVCodecParameters* codecPara = infmt_ctxP->streams[inVStreamIndex]->codecpar;//输入视频流的编码参数
            AVStream* inVStream = infmt_ctxP->streams[inVStreamIndex];


            var fmt_ctxP = ffmpeg.avformat_alloc_context();
          
            ffmpeg.avformat_alloc_output_context2(&fmt_ctxP, null, null, outPath).ThrowExceptionIfError();
            //打开输出文件并填充数据
            ffmpeg.avio_open(&fmt_ctxP->pb, outPath, ffmpeg.AVIO_FLAG_READ_WRITE).ThrowExceptionIfError();
            //在输出的mp4文件中创建一条视频流
            AVStream* outVStream = ffmpeg. avformat_new_stream(fmt_ctxP, null);
           
            outVStream->time_base.den =rate;
            outVStream->time_base.num = 1;
            outVStreamIndex = outVStream->index;
            //查找编码器
         
            AVCodec* outCodec = ffmpeg.avcodec_find_encoder(codecPara->codec_id);
            
            AVCodecContext* outCodecCtx = ffmpeg.avcodec_alloc_context3(outCodec);
            
            AVCodecParameters* outCodecPara = fmt_ctxP->streams[outVStream->index]->codecpar;
            ffmpeg.avcodec_parameters_copy(outCodecPara, codecPara).ThrowExceptionIfError();
            ffmpeg. avcodec_parameters_to_context(outCodecCtx, outCodecPara) .ThrowExceptionIfError();
            outCodecCtx->time_base.den = rate;
            outCodecCtx->time_base.num = 1;

            ffmpeg.avcodec_open2(outCodecCtx, outCodec, null).ThrowExceptionIfError();

            ffmpeg. av_dump_format(fmt_ctxP, 0, outPath, 1);
            ffmpeg.avformat_write_header(fmt_ctxP, null).ThrowExceptionIfError();
            var pkt = ffmpeg.av_packet_alloc();
           
            long frame_index = 0;
            while (ffmpeg.av_read_frame(infmt_ctxP, pkt) >= 0)
            {
                if (pkt->stream_index == inVStreamIndex)
                {
                    if (pkt->pts == ffmpeg. AV_NOPTS_VALUE)
                    {
                        AVRational time_base1 = inVStream->time_base;
                        //Duration between 2 frames (us)
                        var calc_duration = (double)ffmpeg.AV_TIME_BASE / ffmpeg.av_q2d(inVStream->r_frame_rate);
                        //Parameters
                        pkt->pts = (long)((double)(frame_index * calc_duration) / (double)(ffmpeg.av_q2d(time_base1) * ffmpeg.AV_TIME_BASE));
                        pkt->dts = pkt->pts;
                        pkt->duration = (long)((double)calc_duration / (double)(ffmpeg.av_q2d(time_base1) * ffmpeg.AV_TIME_BASE));
                        frame_index++;
                    }
                    pkt->pts = ffmpeg.av_rescale_q_rnd(pkt->pts, inVStream->time_base, outVStream->time_base, ( AVRounding)(AVRounding.AV_ROUND_NEAR_INF | AVRounding.AV_ROUND_PASS_MINMAX));
                    pkt->dts = ffmpeg.av_rescale_q_rnd(pkt->dts, inVStream->time_base, outVStream->time_base, (AVRounding)(AVRounding.AV_ROUND_NEAR_INF | AVRounding.AV_ROUND_PASS_MINMAX));
                    pkt->duration = ffmpeg.av_rescale_q(pkt->duration, inVStream->time_base, outVStream->time_base);
                    if (pkt->duration > 0)
                    {

                    }
                    pkt->pos = -1;
                    pkt->stream_index = outVStreamIndex;
                    ffmpeg.av_interleaved_write_frame(fmt_ctxP, pkt).ThrowExceptionIfError();
                    ffmpeg. av_packet_unref(pkt);
                }
            }
            ffmpeg.av_packet_free(&pkt);
            ffmpeg. av_write_trailer(fmt_ctxP);
            ffmpeg.avio_close(fmt_ctxP->pb);
            ffmpeg.avcodec_close(outCodecCtx);
            ffmpeg.av_free(outCodecCtx);
            ffmpeg.av_free(outCodec);
            ffmpeg.avformat_close_input(&infmt_ctxP);
           // ffmpeg.avformat_free_context(infmt_ctxP);
            ffmpeg.av_free(infmt_ctxP);
            ffmpeg.avformat_free_context(fmt_ctxP);
            //ffmpeg.av_free(fmt_ctxP);
        }

   
    }
}
