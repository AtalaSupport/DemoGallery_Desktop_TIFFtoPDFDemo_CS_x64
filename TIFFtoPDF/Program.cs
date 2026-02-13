using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Atalasoft.Imaging.Codec.Pdf;
using Atalasoft.Imaging.Codec;
using Atalasoft.Imaging;
using System.Windows.Forms;


namespace TIFFtoPDF
{
    class Program
    {
        /// <summary>
        /// Converts any supported input file (MultiPage TIFF) into a multipage PDF file
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            DoAboutSplash();

            Console.WriteLine("TIFFtoPDF starting");


            string imgPath = GetWorkingDir();
            string inFile = imgPath + "target.tif";


            Console.WriteLine("Please select the file to convert to PDF");

            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.InitialDirectory = imgPath;
                dlg.Filter = "Tagged Image File Format (*.TIFF;*.tif)|*.tiff;*.tif;|All Files (*.*)|*.*";
                dlg.FileName = Path.GetFileName(inFile);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    inFile = dlg.FileName;
                }
                else
                {
                    inFile = null;
                }
            }

            if (inFile == null)
            {
                Console.WriteLine("No input file SelectedOperation canceled");
            }
            else
            {
                string outFile = System.IO.Path.ChangeExtension(inFile, ".pdf");

                Console.WriteLine("  inFile: " + inFile);

                Console.WriteLine("Please select a location to save the outgoing PDF to:");
                using (SaveFileDialog dlg = new SaveFileDialog())
                {
                    dlg.Title = "Select Location to Save output PDF to";
                    dlg.Filter = "Portable Document Format (.pdf)|.pdf";
                    dlg.DefaultExt = ".pdf";
                    dlg.InitialDirectory = Path.GetDirectoryName(inFile);
                    dlg.FileName = Path.GetFileName(outFile);

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        outFile = dlg.FileName;
                    }
                    else
                    {
                        // force an error to stop the app
                        outFile = null;
                    }
                }


                if (outFile == null)
                {
                    Console.WriteLine("No Output selcted: Operation canceled");
                }
                else
                {
                    Console.WriteLine("  outFile: " + outFile);
                    Console.WriteLine("Converting file...");


                    // start timer
                    int tick1 = System.Environment.TickCount;

                    // Do the conversion
                    if (File.Exists(inFile))
                    {
                        ConvertTifToPdf(inFile, outFile);
                    }
                    else
                    {
                        Console.WriteLine("file not found... doing nothing");
                    }

                    // finish timer
                    int tick4 = System.Environment.TickCount;

                    Console.WriteLine("Conversion complete: Total time " + (tick4 - tick1) + " ms");

                }
                
            }


            Console.WriteLine("TIFFtoPDF finished... press ENTER to quit");
            Console.ReadLine();
        }


        /// <summary>
        /// Given the filename of a tiff file as input and a pdf file as output, convert the
        /// incoming tiff to a SingleImageOnly PDF
        /// 
        /// This demonstrates the most memory-efficient way to do the conversion.
        /// </summary>
        /// <param name="inFile">A tiff file to use as the input</param>
        /// <param name="outFile">A filename to call the outgoing PDF (note that it will delete any existing file by that name first.</param>
        private static void ConvertTifToPdf(string inFile, string outFile)
        {
            // Make sure we get rid of the output file if it already exists
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }

            // Create your encoder
            PdfEncoder pdfEnc = new PdfEncoder();

            // For Jpeg compression (not Jpeg2000), you can use this value to request 
            // higher compression or quality... the higher the number, the better the 
            // quality, but the less effective the compression. 80 is the default value/
            pdfEnc.JpegQuality = 80;

            //// If you wish to use Jbig2 or Jpeg2000 compression, uncomment the following line to 
            //// instruct the encoder that they're available... NOTE that you will need to add 
            //// the correct references to add such support
            //pdfEnc.UseAdvancedImageCompression = true;
            
            // set up an event handler that will pop for each image to let you determine the compression
            pdfEnc.SetEncoderCompression += new EncoderCompressionEventHandler(pdfEnc_SetEncoderCompression);
            
            // Reading this direct from a FileSystemImageSource will be much more efficient than using an ImageCollection
            using (ImageSource imgSrc = new FileSystemImageSource(inFile, true))
            {
                using (FileStream outStream = new FileStream(outFile, FileMode.Create))
                {
                    // The magic of using an ImageSource and Stream with the pdfEncoder is 
                    // that only the active portion of a given image is in memory at one time
                    // so it's very memory efficient.
                    //
                    // This effectively does   while(imgSrc.HasMoreImages()) { AtalaImage img = imgSrc.AcquireNext(); ... }
                    //
                    // Each page willl trigger a new pdfEnc_SetEncoderCompression event 
                    // where you can choose the type of compression you want to apply on a page-by-page basis
                    pdfEnc.Save(outStream, imgSrc, null);
                }
            }
            
            pdfEnc = null;
        }


        // This event pops for each image (page) as it's saved out... 
        // by setting the e.Compression, you can tell the encoder how to copress for that case
        static void pdfEnc_SetEncoderCompression(object sender, EncoderCompressionEventArgs e)
        {
            if (e.Image.PixelFormat == PixelFormat.Pixel1bppIndexed)
            {
                //// Here, we've chosen CcittGroup4 as our B&W conversion.
                ////
                //// Your viable Advanced compression type would be Jbig2.
                ////
                //// To enable Jbig2, you need to add a reference to 
                //// Atalasoft.dotImage.Jbig2.dll and make sure that 
                //// pdfEnc.UseAdvancedImageCompression = true;   is uncommented above
                ////
                //// NOTE: You must have either a paid or eval license for Jbig2 to use Jbig2 compression
                e.Compression = new PdfCodecCompression(PdfCompressionType.CcittGroup4);
                //e.Compression = new PdfCodecCompression(PdfCompressionType.Jbig2);
            }
            else if (e.Image.PixelFormat == PixelFormat.Pixel8bppGrayscale || e.Image.PixelFormat == PixelFormat.Pixel24bppBgr)
            {
                //// Here, we've chosen Jpeg2000 advanced color compression 
                //// ... Jpeg2000 may be able to handle other types of PixelFormat 
                ////          - simply add them in the else/if statement
                //// your other viable compression tipes would be Jpeg, 

                //// Here, we've chosen Jpeg as our Color and Grayscale conversion.
                ////
                //// Your viable Advanced compression type would be Jpeg2000.
                ////
                //// To enable Jpeg2000, you need to add a reference to 
                //// Atalasoft.dotImage.Jpeg2000.dll and make sure that 
                //// pdfEnc.UseAdvancedImageCompression = true;   is uncommented above 
                ////
                //// NOTE: You must have either a paid or eval license for Jpeg2000 to use Jpeg2000 compression
                e.Compression = new PdfCodecCompression(PdfCompressionType.Jpeg);
                //e.Compression = new PdfCodecCompression(PdfCompressionType.Jpeg2000);
                
            } else {
                // Fallback method in case the pixelFormat isn't one of the ones defined above
                e.Compression = new PdfCodecCompression(PdfCompressionType.Deflate);
            }
        }


        /// <summary>
        /// Convenience method to get the root directory of the project - really only useful for debugging
        /// </summary>
        /// <returns></returns>
        private static string GetWorkingDir()
        {
            string cwd = System.IO.Directory.GetCurrentDirectory();
            //Console.WriteLine("cwd is '{0}'", cwd);

            if (cwd.EndsWith("\\bin\\Debug"))
            {
                cwd = cwd.Replace("\\bin\\Debug", "\\..\\..\\");
                //Console.WriteLine("updated cwd is '{0}'", cwd);
            }
            else if (cwd.EndsWith("\\bin"))
            {
                cwd = cwd.Replace("\\bin", "\\..\\");
                //Console.WriteLine("updated cwd is '{0}'", cwd);
            }
            return cwd;
        }

        /// <summary>
        /// Outputs the "About" spash info
        /// </summary>
        private static void DoAboutSplash()
        {
            Console.WriteLine("TIFFtoPDF Demo");
            Console.WriteLine();
            Console.WriteLine("***************************************************************************");
            Console.WriteLine("A very simple console app that converts a TIFF file into a PDF by");
            Console.WriteLine("using in a memory-efficient way using FileSystemImageSource.");
            Console.WriteLine();
            Console.WriteLine("Who says you always need a viewer in an imaging application?");
            Console.WriteLine();
            Console.WriteLine("This console app uses our FileSystemImageSource to read each frame");
            Console.WriteLine("of the target file directly into a PDF Encoder. Each page read will");
            Console.WriteLine("trigger a Compression Selector event so you can choose the best compression");
            Console.WriteLine("to use for that specific page's Pixel Format");
            Console.WriteLine();
            Console.WriteLine("This approach can easily be adapted to services or plumbed in to");
            Console.WriteLine("batch-based processing.");
            Console.WriteLine();
            Console.WriteLine("By setting a handler for PdfEnc.SetEncoderCompression, we are ");
            Console.WriteLine("able to dynamically select the most appropriate form of image ");
            Console.WriteLine("compression to apply, based on the PixelFormat (color depth) of each page");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Download the DotImage SDK at:");
            Console.WriteLine("     http://www.atalasoft.com/products/download/dotimage");
            Console.WriteLine();
            Console.WriteLine("Download the DotImage API Help Installers at:");
            Console.WriteLine("     http://www.atalasoft.com/support/dotimage/help/install");
            Console.WriteLine();
            Console.WriteLine("Download the full sources for this demo at:");
            Console.WriteLine("     http://www.atalasoft.com/KB/article.aspx?id=10412");
            Console.WriteLine("***************************************************************************");
            Console.WriteLine();
        }
    }
}
