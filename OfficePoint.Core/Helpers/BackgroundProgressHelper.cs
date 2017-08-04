using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;

namespace OfficePoint.Core.Helpers
{
    public static class BackgroundProgressHelper
    {
        public static async void LaunchAppInForeground(VoiceCommandServiceConnection voiceServiceConnection, VoiceCommandType commandType, string parameters)
        {
            var userMessage = new VoiceCommandUserMessage();

            userMessage.DisplayMessage = "Launching OfficePoint...";

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            response.AppLaunchArgument = "type=" + commandType + "&" + parameters;

            await voiceServiceConnection.RequestAppLaunchAsync(response);
        }

        public static async void LaunchAppInForeground(VoiceCommandServiceConnection voiceServiceConnection)
        {
            var userMessage = new VoiceCommandUserMessage();

            userMessage.DisplayMessage = "Launching OfficePoint...";

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            response.AppLaunchArgument = "type=" + VoiceCommandType.None;

            await voiceServiceConnection.RequestAppLaunchAsync(response);
        }

        public static async Task ReportFailureAsync(VoiceCommandServiceConnection voiceServiceConnection, string message)
        {
            var userMessage = new VoiceCommandUserMessage();

            string noSuchTripToDestination = message;
            userMessage.DisplayMessage = userMessage.SpokenMessage = noSuchTripToDestination;

            var response = VoiceCommandResponse.CreateResponse(userMessage);
            await voiceServiceConnection.ReportFailureAsync(response);

            return;
        }

        public static async Task ShowProgressScreen(VoiceCommandServiceConnection voiceServiceConnection, string message)
        {
            var userProgressMessage = new VoiceCommandUserMessage();
            userProgressMessage.DisplayMessage = userProgressMessage.SpokenMessage = message;

            //METHODS TO RESPOND TO CORTANA TO SEND A BASIC MESSAGE
            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userProgressMessage);
            await voiceServiceConnection.ReportProgressAsync(response);

            return;
        }
    }
}
