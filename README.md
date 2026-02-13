# TIFFtoPDFDemo
A very simple console app that converts a TIFF file into a PDF by
using in a memory-efficient way using FileSystemImageSource.

Who says you always need a viewer in an imaging application?

This console app uses our FileSystemImageSource to read each frame
of the target file directly into a PDF Encoder. Each page read will
trigger a Compression Selector event so you can choose the best compression
to use for that specific page's Pixel Format

This approach can easily be adapted to services or plumbed in to
batch-based processing.

By setting a handler for PdfEnc.SetEncoderCompression, we are 
able to dynamically select the most appropriate form of image 
compression to apply, based on the PixelFormat (color depth) of each page.

NOTE that tecnically, the input file is not limited to TIfF, any supported 
input file type may be used. Some require additional references and to have 
additional decoders added to RegisteredDecoders.Decoders. Please feel free 
to reach out to support if you have questions.

## Prerequisites
This demo assumes you have the Atalasoft DotImage SDK installed and 
licensed for DotImage Document Imaging.

You may also request a 30 day evaluation when installing / activating

[Download DotImage](https://www.atalasoft.com/BeginDownload/DotImageDownloadPage)

## Cloning
We recommend cloning to a local directory

Example: git for windows
```bash
git clone https://github.com/AtalaSupport/DemoGallery_Desktop_TIFFtoPDFDemo_CS_x64.git TIFFtoPDFDemo
cd TIFFtoPDFDemo
```
