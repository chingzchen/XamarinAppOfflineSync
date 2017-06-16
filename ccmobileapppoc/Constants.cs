using System;

namespace ccmobileapppoc
{
	public static class Constants
	{
        // Replace strings with your Azure Mobile App endpoint.
        public static string ApplicationURL = @"https://ccmobileapppoc.azurewebsites.net";
        public const string SenderID = "217712605294"; // Google API Project Number
        public const string ListenConnectionString = "Endpoint=sb://ccmobileapppoc.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=jL09hLNJy6+qy5YDLuw3ytYsepFMgWXmJEU2+R9sGtU=";
        public const string NotificationHubName = "ccmobileapppochub";
        public const string TAG = "MyBroadcastReceiver-GCM";
    }
}

