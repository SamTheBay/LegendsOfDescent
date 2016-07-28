using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

#if WIN8
using Windows.Storage;
using Windows.ApplicationModel.Store;
using Windows.ApplicationModel;
using System.Threading.Tasks;
#endif

namespace LegendsOfDescent
{
    public class ScreenShot
    {
        private static RenderTarget2D ss;

        static public void StartScreenShot()
        {
            // capture the back buffer
            ss = new RenderTarget2D(DungeonGame.graphics.GraphicsDevice, DungeonGame.ScreenSize.Width, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y);
            DungeonGame.graphics.GraphicsDevice.SetRenderTarget(ss);
        }


        static public void EndScreenShot()
        {
            DungeonGame.graphics.GraphicsDevice.SetRenderTarget(null);


            try
            {
                // write the image to the isolated storage
                if (ss != null)
                {
                    string longFileName = "LoD_SS_" +
                                            DateTime.Now.Year.ToString() +
                                            DateTime.Now.Day.ToString() +
                                            DateTime.Now.Hour.ToString() +
                                            DateTime.Now.Minute.ToString() +
                                            DateTime.Now.Second.ToString();


#if WINDOWS_PHONE
                    longFileName = longFileName + ".jpeg";
                    using (var stream = Storage.OpenCreate("LoD_SS.jpeg"))
                    {
                        // Save the image to the saved pictures album.
                        MediaLibrary library = new MediaLibrary();
                        library.SavePicture(longFileName, stream);
                    }
#else
#if WIN8
                    Task.Run(async () =>
                    {
                        try
                        {

                            StorageFolder proxyDataFolder = null;
                            StorageFile proxyFile = null;
                            longFileName = longFileName + ".png";
                            proxyDataFolder = KnownFolders.PicturesLibrary;
                            proxyFile = await proxyDataFolder.CreateFileAsync(longFileName);
                            Stream stream = await proxyFile.OpenStreamForWriteAsync();
                            using (stream)
                            {
                                ss.SaveAsPng(stream, ss.Width, ss.Height);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                        }
                    }).Wait();

#else
                    longFileName = longFileName + ".jpeg";
                    using (var stream = Storage.OpenWrite(longFileName))
                    {
                        ss.SaveAsJpeg(stream, ss.Width, ss.Height);
                    }
#endif
#endif

                    // draw the screenshot to the real back buffer to prevent a screen flicker
                    SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
                    spriteBatch.Begin();
                    spriteBatch.Draw(ss, Vector2.Zero, Color.White);
                    spriteBatch.End();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            ss = null;
        }


    }
}
