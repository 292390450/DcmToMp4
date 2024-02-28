# DicomToMp4
dicom File export to mp4 [NuGet](https://www.nuget.org/packages/DicomToMp4/)
## use
1.Set the ffmpeg dynamic library directory

`ffmpeg.RootPath =Path.Combine( AppContext.BaseDirectory, "NativeApi");`

2.invoke

` DcmExporter.Export(string sourcePath,string destPath,int width=0,int height=0,int frameRate=10)`


>You can install the corresponding package version according to your ffmpeg version
[[NuGet]](https://www.nuget.org/packages/FFmpeg.AutoGen)
