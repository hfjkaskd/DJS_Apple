namespace WZSDK
{
	public interface IBaseView
	{
		int ViewId { get; set; }

		bool IsInit();

		bool IsShow();

		void InitUi();

		void InitData();

		void Open(params object[] args);

		void Close(params object[] args);

		void DestroyView();

		void SetVisible(bool value);
	}
}
