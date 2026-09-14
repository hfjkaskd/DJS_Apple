public class HOPKEMDHFIM
{
	public interface CPHPOKCFEPP
	{
		void onAdLoaded(string network, string msg);

		void onAdFailedToLoad(string msg);

		void onAdClicked(string network, string msg);
	}

	public interface PNCNLFMLLPF
	{
		void onAdLoaded(string network, double value, string msg);

		void onAdFailedToLoad(string msg);

		void onAdShowed(string network, string msg);

		void onAdFailedToShow(string network, int error_code, string error);

		void onAdClosed(string network);

		void onAdClicked(string network, string msg);

		void onAdInfoChanged(string network, double value);
	}

	public interface AJOMGAHFFLB
	{
		void onAdLoaded(string network, double value, string msg);

		void onAdFailedToLoad(string msg);

		void onAdShowed(string network, string msg);

		void onAdFailedToShow(string network, int error_code, string error);

		void onAdRewarded(string network, string msg);

		void onAdClosed(string network);

		void onAdClicked(string network, string msg);

		void onAdInfoChanged(string network, double value);
	}
}
