using Android.App;
using Android.Content.PM;
using Android.OS;
using System;
using Gcm.Client;

namespace XamarinMobileAppPush.Droid
{
    [Activity(Label = "XamarinMobileAppPush", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        // Create a new instance field for this activity.
        static MainActivity instance = null;

        // Devuleve la activity actual
        public static MainActivity CurrentActivity
        {
            get
            {
                return instance;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            // Asignamos la acutal instacia de MainAcivity
            instance = this;
            //Registramos las notificaciones
            RegisterNotificationsPush();

            base.OnCreate(bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        private void RegisterNotificationsPush()
        {
            try
            {
                // Validamos que todo este correcto para poder aplicar notificaciones Push
                GcmClient.CheckDevice(this);
                GcmClient.CheckManifest(this);
                // Registramos el dispositivo para poder recibir notificaciones Push
                GcmClient.Register(this, PushHandlerBroadcastReceiver.SENDER_IDS);
            }
            catch (Java.Net.MalformedURLException)
            {
                CreateAndShowDialog("There was an error creating the client. Verify the URL.", "Error");
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e.Message, "Error");
            }
        }

        private void CreateAndShowDialog(String message, String title)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }
    }
}

