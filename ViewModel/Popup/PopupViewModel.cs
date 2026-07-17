using System.Windows;
using System.Windows.Threading;
using TCM_Launcher.Interfaces;
using TCM_Launcher.MVVM;
using TCM_Launcher.Services;

namespace TCM_Launcher.ViewModel.Popup
{
    public enum PopupAction
    {
        CRASH, UPDATE
    }

    public class PopupViewModel : ViewModelBase
    {
		private Window caller;
        private DateTime targetEndTime;
        private readonly IGameProfileService gameProfileService;
        public PopupViewModel(IGameProfileService gameProfileService)
        {
			this.gameProfileService = gameProfileService;
        }

		private DispatcherTimer timer;
		private string? profileId;
        private PopupAction popupType;

        private string labelText;
		public string LabelText
		{
			get { return labelText; }
			set 
			{ 
				labelText = value;
				OnPropertyChange();
			}
		}

		private string text;
		public string Text
		{
			get { return text; }
			set 
			{ 
				text = value;
				OnPropertyChange();
			}
		}

		private double? remainTime;
		public double? RemainTime
		{
			get { return remainTime; }
			set 
			{
				remainTime = value;
				OnPropertyChange();
			}
		}

        private double? maxRemainTime;
        public double? MaxRemainTime
        {
            get { return maxRemainTime; }
            set
            {
                maxRemainTime = value;
                OnPropertyChange();
            }
        }

        private string buttonText;
        public string ButtonText
        {
            get { return buttonText; }
            set 
            { 
                buttonText = value;
                OnPropertyChange();
            }
        }

        public void Initialize(Window caller, string lText, string t, PopupAction type, double? rTime = null, string? profileId = null)
        {
            this.caller = caller;
            LabelText = lText;
            Text = t;
            popupType = type;
            switch (popupType)
            {
                case PopupAction.UPDATE:
                    ButtonText = "Update";
                    break;
                case PopupAction.CRASH:
                    ButtonText = "Open crash folder";
                    break;
            }
            RemainTime = rTime;
            MaxRemainTime = rTime;
            this.profileId = profileId;
            if (rTime != null) StartTimer();
        }

        private void StartTimer()
        {
            targetEndTime = DateTime.Now.AddMilliseconds(MaxRemainTime ?? 3000d);

            timer = new DispatcherTimer(DispatcherPriority.Render);

            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += TimerTick;
            timer.Start();
        }

        private void TimerTick(object sender, EventArgs e) 
		{
            double timeLeft = (targetEndTime - DateTime.Now).TotalMilliseconds;

            if (timeLeft <= 0)
            {
                RemainTime = 0;
                timer.Stop();
                caller.Close();
            }
            else
            {
                RemainTime = timeLeft;
            }
        }

        public void ManageActions()
        {
            switch (popupType)
            {
                case PopupAction.CRASH:
                    OpenCRASH();
                    break;
                case PopupAction.UPDATE:
                    Update();
                    break;
            }
        }

		public void OpenCRASH()
		{
			gameProfileService.OpenProfileFolder(profileId, "crash-reports");
        }

        public void Update()
        {
            caller.DialogResult = true;
            caller.Close(); 
        }
    }
}
