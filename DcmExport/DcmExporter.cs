using System;
using System.Drawing;
using System.IO;
using FellowOakDicom.Imaging;
using FFmpeg.AutoGen;

namespace DcmExport
{
    public class DcmExporter
    {
        public unsafe static void Export(string sourcePath,string destPath,int width=0,int height=0,int frameRate=10)
        {
           // ffmpeg.RootPath =Path.Combine( AppContext.BaseDirectory, "NativeApi");
            DicomImage dicomImage = new DicomImage(sourcePath);
            if (width==0||height==0)
            {
                width = dicomImage.Width;
                height = dicomImage.Height;
            }
            var frameConverter = new VideoFrameConverter(new Size(dicomImage.Width, dicomImage.Height),
                AVPixelFormat.AV_PIX_FMT_BGRA, new Size(width, height), AVPixelFormat.AV_PIX_FMT_YUV420P);

           // var recorder = DBBSMCoreAPI.CreateRecord(destPath, width, height, 44100);
           
            //var endocder = new H264Encoder(frameRate, new Size(width, height), ((ptr, i, arg3, arg4, arg5, arg6) =>
            //{
            //   // DBBSMCoreAPI.FillFrameRecordVideo(recorder,ptr,i,arg5);
            //}));
            // ex = new Mp4Export(destPath,endocder);
            using (var fs = File.Open(destPath+".h264", FileMode.Create))
            {
                using (var vse = new H264VideoStreamEncoder(fs, frameRate, new Size(width, height)))
                {
                    for (int i = 0; i < dicomImage.NumberOfFrames; i++)
                    {
                        using (var image = dicomImage.RenderImage(i))
                        {
                            if (frameConverter.SourceSize.Height != image.Height || frameConverter.SourceSize.Width != image.Width)
                            {
                                frameConverter.Dispose();
                                frameConverter = new VideoFrameConverter(new Size(image.Width, image.Height),
                                    AVPixelFormat.AV_PIX_FMT_BGRA, new Size(width, height), AVPixelFormat.AV_PIX_FMT_YUV420P);
                            }

                            var data = new byte_ptrArray8
                                { [0] = (byte*)image.Pixels.Pointer };
                            var linesize = new int_array8 { [0] = image.Pixels.ByteSize / image.Height, };
                            AVFrame rgbFrame = new AVFrame()
                            {
                                data = data,
                                linesize = linesize,
                                height = image.Height
                            };
                            var yuv420 = frameConverter.Convert(rgbFrame);
                            yuv420.pts = i * frameRate;
                            vse.Encode(yuv420);
                            // endocder.Encode(yuv420);

                           

                        }
                    }
                    vse.Drain();
                }
                //  DBBSMCoreAPI.StartRecord(recorder);
            }
            frameConverter.Dispose();
            Mp4Export mp4Export = new Mp4Export(destPath + ".h264", destPath, frameRate);
            File.Delete(destPath + ".h264");
            // Thread.Sleep(TimeSpan.FromMilliseconds(perTime));
            //ex.End();
            // DBBSMCoreAPI.StopRecord(recorder);
            // DBBSMCoreAPI.CloseRecord(recorder);
            //endocder.Dispose();
        }
    }
}
