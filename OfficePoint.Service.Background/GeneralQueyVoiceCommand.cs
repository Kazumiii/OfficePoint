using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Media.SpeechRecognition;
using Windows.ApplicationModel.Core;
using System.Globalization;

namespace OfficePoint.Service.Background
{
    //In XML file I told cortana to maps to this class
    //so I must sign this class like seald and implement IBackgorundTask interface 
    public sealed  class GeneralQueyVoiceCommand:IBackgroundTask
    {
        //Connection to Cortana communicataion
        VoiceCommandServiceConnection voiceServiceConnection;

        //runnning both in the background and asycnchronicly
        //we mustt grab it to further proccessing and then complet it 
        BackgroundTaskDeferral taskDeferall;


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            taskDeferall = taskInstance.GetDeferral();

            //we add OnTaskCanceled event
            taskInstance.Canceled += OnTaskCanceled;

            //grab triggerDetails and cast on ApppService type 
            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            try
            {
                //here we creating connection which has all our information
                voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);

                //Add CompletedEvent without it Cortand will hang up very often that's why Cortans won't know when task is completed
                //I divided  Completed into two ways  one: TaskCompleted and second VoiceCommand Completed to let Cortana know when task and command is completed 
                voiceServiceConnection.OnVoiceCommandCompleted += OnVoiceCommandCompleted;

                //voiceCommand is part of voiceSerivceConnection  I'm gonna to use it for....
                VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                //grab interpretation
                //Interpreatin is very important to inform Cortan what we want, it's all we care about 
                var interpretation = voiceCommand.SpeechRecognitionResult.SemanticInterpretation;
                VoiceCommandType commandType = (VoiceCommandType)Enum.Parse(typeof(VoiceCommandType), voiceCommand.CommandName, true);
                switch(commandType)
                {
                    case VoiceCommandType.InterestingFactQueryCommand:
                        await ProcessInterestingFactAsync(interpretation);
                        break;
                    case VoiceCommandType.WeekOfYearQueryCommand:
                        await ProcessWeekOfYearAsync(interpretation);
                        break;

                    case VoiceCommandType.FindBusinessQueryCommand:
                        ProcessFindBusinessAsync(interpretation);
                        break;

                    default:
                        break;
                }

            }
            catch(Exception ex)
            {

            }



        }


       




        private async Task ProcessInterestingFactAsync(SpeechRecognitionSemanticInterpretation interpretation)
        {
            await Core.Helpers.BackgroundProgressHelper.ShowProgressScreen(voiceServiceConnection, "Okay,get ready");

            //here is this what I want to cortana will tell
            string fact = await Core.Helpers.FactHelper.GetRandomFactAsync();
            var DestinationContentTiles = new List<VoiceCommandContentTile>();
            var destinationTile = new VoiceCommandContentTile();
            try
            {

                //What style we want Cortana shows us , size tile that will be displayed
                destinationTile.ContentTileType = VoiceCommandContentTileType.TitleWith280x140IconAndText;
                //What images should be inside of tiles
                destinationTile.Image = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///OfficePoint.Service.Background/Images/Fact_280.png"));


                //add to the VoiceCommandContenTile, and say how you handle sending a single  respone back
                destinationTile.AppContext = null;

                //load up in the user interface
                destinationTile.AppLaunchArgument = "type=" + VoiceCommandType.InterestingQueryFact;

                destinationTile.Title = fact;

                //here is what i Want to cortan will write
                destinationTile.TextLine1 = "";

                DestinationContentTiles.Add(destinationTile);


                    }

            catch(Exception ex)
            {

            }

            //here I'm creating my response  
            VoiceCommandResponse voiceRespone = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage()
            {
                // what cortan write
                DisplayMessage = "did you know...",
                //randomly fact whih is chosen perviously which is speaking by Cortana
                SpokenMessage = fact,
            }, DestinationContentTiles);


            //voiceServiceConnection is connection to Cortana here is using to send our response 
            await voiceServiceConnection.ReportSuccessAsync(voiceRespone);


        }


        //Very similar like above
        private async Task ProcessWeekOfYearAsync(SpeechRecognitionSemanticInterpretation interpretation)
        {

            DateTimeFormatInfo dfi = new DateTimeFormatInfo.CurrentInfo;

            Calendar cal = dfi.Calendar;

            var firstDay = cal.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstDay, dfi.FirstDayOfWeek);

            var firstFourDay = cal.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstFourDayWeek, dfi.FirstDayOfWeek);

            var firstFullDay = cal.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstFullWeek, dfi.FirstDayOfWeek);


            string fullDayLabel = "Today is week:" + firstDay + "by the first day rule" + firstFourDay;

            var destinationContentTile = new List<VoiceCommandContentTile>();

            var destinationTile = new VoiceCommandContentTile();

            try
            {

                destinationTile.ContentTileType = VoiceCommandContentTileType.TitleWithText;
                destinationTile.AppContext = null;
                destinationTile.AppLaunchArgument = "type=" + VoiceCommandType.WeekOfYearQueryCommand;
                destinationTile.Title = DateTime.Today.ToString(dfi.LongDatePattern);

                destinationTile.TextLine1 = "today is week #" + firstDay + "by the first day rule";
                destinationTile.TextLine2 = "Week #" + firstFourDay + "by the first four day rule";
                destinationTile.TextLine3 = " Week #" + firstFullDay + "by the first full week rule";

                destinationContentTile.Add(destinationTile);


            }

            catch(Exception ex)
            {

            }

            VoiceCommandResponse response2 =  VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage
            {
                DisplayMessage = "Week of the year",
                SpokenMessage = fullDayLabel,

            }, destinationContentTile);

            await voiceServiceConnection.ReportSuccessAsync(response2);
        }



        //This method is going use to Bing location search API and find bussines based on dictated text
        //for instnace  we can find a coffe shope around here
        private async Task ProcessFindBusinessAsync(SpeechRecognitionSemanticInterpretation interpretation)
        {
            string searchQuery = null;

            //It contains everything what we want to find, from this we pass value whch is using to find what we want
            //dictatedFindBusiness value is using to pass this what we  we want to find 
            //this name must be the same like lable in phrase list 
            if(interpretation.Properties.ContainsKey("dictatedFindBusinessText") && interpretation.Properties["dictatedFindBusinessText"].Count>0)
            {
                searchQuery = interpretation.Properties["dictatedFindBusinessText"].First().Trim();

            }
            if(!string.IsNullOrWhiteSpace(searchQuery))
            {
                await Core.Helpers.BackgroundProgressHelper.ShowProgressScreen(voiceServiceConnection, "Searching for" + searchQuery + "near you");


                //Taking our location to make sure  location is available for us  and send it to...
                var currentLocation-await Core.Helpers.LocationHelper.GetCurrentLocationAsync();

                LocationResultInformation selectedLocationResault = null;

                if(currentLocation!=null)
                {
                    double latitiude = currentLocation.Point.Position.Latitiude;
                    double longitiude = currentLocation.Point.Position.Longitiude;


                    //  this section, here  we  use a Bing API 
                    var locationResults = await Core.Helpers.SearchHelper.SearchLocation(searchQuery, latitiude, longitiude);

                   

                    //Cortand is able to  recognize devices so if we use WIndows Phone we want to take only five results
                    if(Helpers.PlatformHelper.IsWindowsPhone)
                    {
                       locationResults = LocationResult.Take(5).ToList();


                    }

                    //if i got ober 1 result use Disambiguate function 
                    if (locationResults.Count > 1)
                    {
                        //here we using Disambiguations functon which contains list locations we want ot find, we 're talking about
                        selectedLocationResault = await DisambiguateLocations(locationResults, searchQuery);

                        var userMassage = new VoiceCommandUserMessage();

                        userMassage.DisplayMessage = "Loading..." + selectedLocationResault.DisplayName + ".";
                        userMassage.SpokenMessage = "Loading..." + selectedLocationResault.Displayname + ".";


                        Core.Helpers.BacgroundProgressHelpers.LaunchAppInForeground(voiceServiceConnection, VoiceCommandType)

                    }
                    else if (locationResults == 1)
                    {
                        List<VoiceCommandContentTile> destinationContentTitle = new List<VoiceCommandContentTile>();

                        VoiceCommandContentTile destitantionTile = null;

                        selectedLocationResault = locationResults.FirstOrDefault();

                        destitantionTile = new VoiceCommandContentTile();

                        destitantionTile.ContentTileType = VoiceCommandContentTileType.TitleWithText;

                        destitantionTile.Title = locationResult.DisplayName;

                        destitantionTile.TextLine1 = locationResult.StreetAddress;

                        destitantionTile.TextLine2 = ((double)locationResult.DistanceAway).ToString("N2") + "miles away";

                        destitantionTile.AppContext = selectedLocationResault;

                        destitantionTile.AppLaunchArgument = "type:" + VoiceCommandType.FindBusinessQueryCommand + "&searchQuery";

                        destinationContentTitle.Add(destitantionTile);


                        var response = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage()
                        {
                            DisplayMessage = searchQuery.ToUpper() + "found",
                            SpokenMessage = "I found" + searchQuery,

                        },destinationContentTitle);


                        await voiceServiceConnection.ReportSuccessAsync(response);
                    }




                    }
                }
            }
        }

        //here we keep list of  our locations
        private async Task<LocationResultInformation> DisambiguateLocations(List<LocationResultInformation> leocationResult)
        {
        //cortan says it  at hte begining to user
            var userPrompt = new VoiceCommandUserMessage();
            userPrompt.DisplayMessage = userPrompt.SpokenMessage = "Select a search result";


        //If user don't respond coratna will say this
            var userRePrompt = new VoiceCommandUserMessage();
            userRePrompt.DisplayMessage = userRePrompt.SpokenMessage = "Seriously,select a search result";

            List<VoiceCommandContentTile> destinationContentTitle = new List<VoiceCommandContentTile>();

            VoiceCommandContentTile destitantionTile = null;


        //standard configuration
            foreach(var locationResult in leocationResult)
            {

                destitantionTile = new VoiceCommandContentTile();

                destitantionTile.ContentTileType = VoiceCommandContentTileType.TitleWithText;

                destitantionTile.Title = locationResult.DisplayName;

                destitantionTile.TextLine1 = locationResult.StreetAddress;

                destitantionTile.TextLine2 = ((double)locationResult.DistanceAway).ToString("N2") + "miles away";

                destitantionTile.AppContext = locationResult;

            destitantionTile.AppLaunchArgument = "type=" + VoiceCommandType.FindBusinessQueryCommand + "&searchQuery";

            destinationContentTitle.Add(destitantionTile);

            var Response = VoiceCommandResponse.CreateResponse(userPrompt, userRePrompt, destinationContentTitle);

            try
            {
                var voiceCommandDiasmbiguationsResult = await voiceServiceConnection.RequestDisambiguationAsync(Response);
                if(voiceCommandDiasmbiguationsResult!=null)
                {

                    return (LocationResultInformation)voiceCommandDiasmbiguationsResult.SelectedItem.AppContext;


                }
            }

            catch(Exception ex)
            {


            }

            }



        }

    private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
    {
        if (this.taskDeferral != null)
        {
            this.taskDeferral.Complete();
        }
    }

    private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
    {
        if (this.taskDeferral != null)
        {
            this.taskDeferral.Complete();
        }
    }



}
}
