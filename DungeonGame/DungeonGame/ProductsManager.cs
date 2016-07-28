using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Storage;
using Windows.ApplicationModel.Store;
using Windows.ApplicationModel;

namespace LegendsOfDescent
{
    public class ProductsManager
    {
        private ListingInformation listings;
        private static LicenseInformation licenseInformation;
        private String productBuy = null;

        public ProductsManager()
        {
            try
            {
#if DEBUG
                // Setup the debug product data
                Task.Run(async () =>
                {
                    StorageFolder proxyDataFolder = null;
                    StorageFile proxyFile = null;
                    try
                    {
                        proxyDataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Assets\\Data");
                        proxyFile = await proxyDataFolder.GetFileAsync("WindowsStoreProxy.xml");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                    await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
                }).Wait();
#endif

                Task.Run(async () =>
                {

                    try
                    {
#if DEBUG
                        listings = await CurrentAppSimulator.LoadListingInformationAsync();
#else
                        listings = await CurrentApp.LoadListingInformationAsync();
#endif
                    }
                    catch (AggregateException ex)
                    {
                        // this occurs in retail mode when testing
                        Debug.WriteLine(ex.Message);
                    }
                }).Wait();


#if DEBUG
                licenseInformation = CurrentAppSimulator.LicenseInformation;
#else
                licenseInformation = CurrentApp.LicenseInformation;
#endif
                licenseInformation.LicenseChanged += licenseInformation_LicenseChanged;
            }
            catch (AggregateException ex)
            {
                // this occurs in retail mode when testing
                Debug.WriteLine(ex.Message);
            }

        }

        void licenseInformation_LicenseChanged()
        {
            RefreshLicenseState();
        }


        public void RefreshLicenseState()
        {
            if (licenseInformation == null)
                return;

            // handle license changes for the app
            if (licenseInformation.ProductLicenses["Disable Ads"].IsActive)
            {
                DungeonGame.paid = true;
                DungeonGame.adControlManager.ShowAds = false;
            }
            if (SaveGameManager.CurrentPlayer != null)
            {
                if (licenseInformation.ProductLicenses["Stash Slot 5"].IsActive)
                {
                    SaveGameManager.CurrentPlayer.StashesUnlocked = 5;
                }
                else if (licenseInformation.ProductLicenses["Stash Slot 4"].IsActive)
                {
                    SaveGameManager.CurrentPlayer.StashesUnlocked = 4;
                }
                else if (licenseInformation.ProductLicenses["Stash Slot 3"].IsActive)
                {
                    SaveGameManager.CurrentPlayer.StashesUnlocked = 3;
                }
                else if (licenseInformation.ProductLicenses["Stash Slot 2"].IsActive)
                {
                    SaveGameManager.CurrentPlayer.StashesUnlocked = 2;
                }
                else
                {
                    SaveGameManager.CurrentPlayer.StashesUnlocked = 1;
                }
            }
        }


        public void PurchaseLicense(string id)
        {
            productBuy = id;
        }


        public string GetNextPurchase()
        {
            string id = productBuy;
            productBuy = null;
            return id;
        }


        public bool IsLiscenseValid(string id)
        {
            return licenseInformation != null ? licenseInformation.ProductLicenses[id].IsActive : false;
        }

    }
}
