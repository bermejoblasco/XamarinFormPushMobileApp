using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.App;
using Android.Util;
using Gcm.Client;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

//A�adimos los permisos necesarios para utilizar notificaciones push
[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]

namespace XamarinMobileAppPush.Droid
{
    //Indicamos que es un servicio que se ejecutar� en background
    [Service]
    //Debe derivar de la clase GcmServiceBase
    public class GcmService : GcmServiceBase
    {
        public static string RegistrationID { get; private set; }

        //Constructor que llamar� a su constructor base pas�ndole los sernder id's de nuestro GCM creado.
        public GcmService() : base(PushHandlerBroadcastReceiver.SENDER_IDS) { }

        //Sobreescribimos el m�todo OnRegistered que se llamar� una vez el dispositivo ha sido registrado
        protected override void OnRegistered(Context context, string registrationId)
        {
            try
            {
                RegistrationID = registrationId;
                //Una vez tenemos el id de regsitro podemos registrarnos a las notificaciones push de nuestra mobile app
                var push = ServiceManager.DefaultManager.CurrentClient.GetPush();
                MainActivity.CurrentActivity.RunOnUiThread(() => Register(push, null));
            }
            catch (Exception ex)
            {
                Log.Error("Exception", ex.Message);
            }
        }

        public async void Register(Microsoft.WindowsAzure.MobileServices.Push push, IEnumerable<string> tags)
        {
            try
            {
                await push.RegisterAsync(RegistrationID);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                Debugger.Break();
            }
        }

        //Sobreescribimos el m�todo OnMessage que se llamar� cuando se reciba una notificaci�n push
        protected override void OnMessage(Context context, Intent intent)
        {
            var msg = new StringBuilder();

            if (intent != null && intent.Extras != null)
            {
                foreach (var key in intent.Extras.KeySet())
                {
                    msg.AppendLine(key + "=" + intent.Extras.Get(key).ToString());
                }
            }

            var prefs = GetSharedPreferences(context.PackageName, FileCreationMode.Private);
            var edit = prefs.Edit();
            edit.PutString("last_msg", msg.ToString());
            edit.Commit();

            string message = intent.Extras.GetString("message");            
            createNotification("Xamain Push", "Message: " + message);         
        }

        void createNotification(string title, string desc)
        {
            //Creamos la notificaci�n
            var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            //Creamos el intent para mostrar el mensjae
            var uiIntent = new Intent(this, typeof(MainActivity));

            //Notification Builder
            NotificationCompat.Builder builder = new NotificationCompat.Builder(this);

           //Creamos el mensaje
            var notification = builder.SetContentIntent(PendingIntent.GetActivity(this, 0, uiIntent, 0))
                    .SetSmallIcon(Android.Resource.Drawable.SymActionEmail)
                    .SetTicker(title)
                    .SetContentTitle(title)
                    .SetContentText(desc)
                    .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                    .SetAutoCancel(true).Build();

            //Mostramos la notificaci�n
            notificationManager.Notify(1, notification);
        }

        //Sobreescribimos el m�todo OnMessage que se llamar� cuando se reciba una notificaci�n push
        protected override void OnUnRegistered(Context context, string registrationId)
        {
            Log.Error("PushHandlerBroadcastReceiver", "Unregistered RegisterationId : " + registrationId);
        }

        //Sobreescribimos el m�todo OnMessage que se llamar� cuando se reciba una notificaci�n push
        protected override void OnError(Context context, string errorId)
        {
            Log.Error("PushHandlerBroadcastReceiver", "GCM Error: " + errorId);
        }
    }
}