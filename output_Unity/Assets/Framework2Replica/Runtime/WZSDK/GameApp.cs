namespace WZSDK
{
	public class GameApp : SingleTon<GameApp>
	{
		public static ViewManager viewManager;

		public static BasePanel BasePanel;

		public override void Init()
		{
			viewManager = new ViewManager();
			BasePanel = new BasePanel();
		}
	}
}
