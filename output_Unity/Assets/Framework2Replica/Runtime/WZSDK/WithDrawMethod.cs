using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WithDrawMethod : BaseViewWithDraw
	{
		private enum PixKeyType
		{
			CpfCnpj = 0,
			Phone = 1,
			Email = 2,
			Random = 3
		}

		private static readonly Regex EmailRegex = new Regex(
			"^[a-zA-Z0-9._%+\\-]+@[a-zA-Z0-9.\\-]+\\.[a-zA-Z]{2,}$",
			RegexOptions.IgnoreCase);

		private const string RetryWithdrawMissingEmailText =
			"邮件为空！";

		private const string MissingPixKeyText =
			"Informe sua Chave Pix.";

		private Toggle toggleA;

		private Toggle toggleB;

		private Toggle toggleC;

		private GameObject email;

		private GameObject nameObj;

		private GameObject phoneFront;

		private GameObject phoneBehind;

		private InputField emailInputField;

		private InputField phoneInputField;

		private InputField nameInputField;

		private GameObject brazilPixControls;

		private Toggle pixCpfCnpjToggle;

		private Toggle pixPhoneToggle;

		private Toggle pixEmailToggle;

		private Toggle pixRandomToggle;

		private Text pixKeyErrorText;

		private PixKeyType selectedPixKeyType;

		private InputField.ContentType defaultEmailContentType;

		private InputField.InputType defaultEmailInputType;

		private TouchScreenKeyboardType defaultEmailKeyboardType;

		private InputField.CharacterValidation
			defaultEmailCharacterValidation;

		private int defaultEmailCharacterLimit;

		private bool defaultInputPresentationCaptured;

		private bool defaultNameActive;

		private ToggleGroup toggleGroup;

		private string playerName;

		private string phone;

		private string emailAddr;

		private GameObject panelE;

		private double money;

		private int level;

		private int num;

		private DataModel dataModel;

		private Button confirmButton;

		private Button closeButton;

		private int index;

		private bool uiBound;

		private bool isRetryWithdrawSubmitting;

		protected override void OnAwake()
		{
			BindUi();
		}

		protected override void HandleViewArgs(object[] args)
		{
			BindUi();
			dataModel = args != null && args.Length > 0
				? args[0] as DataModel
				: null;
			if (dataModel == null)
			{
				dataModel = new DataModel();
			}

			money = dataModel.Money;
			level = dataModel.Level;
			GameManagerWZ gameManager = GameManagerWZ.instance;
			playerName = !string.IsNullOrEmpty(dataModel.Name)
				? dataModel.Name
				: gameManager != null
					? gameManager.getPlayerName()
					: string.Empty;
			phone = !string.IsNullOrEmpty(dataModel.Phone)
				? dataModel.Phone
				: gameManager != null
					? gameManager.getPlayerPhone()
					: string.Empty;
			string resolvedEmail = !string.IsNullOrEmpty(dataModel.Email)
				? dataModel.Email
				: gameManager != null
					? gameManager.getPlayerEmail()
					: string.Empty;

			playerName = playerName ?? string.Empty;
			phone = phone ?? string.Empty;
			if (IsBrazilianPaymentMethod())
			{
				selectedPixKeyType = PixKeyType.CpfCnpj;
				emailAddr = resolvedEmail ?? string.Empty;
			}
			else
			{
				emailAddr = SanitizeReceiverInput(resolvedEmail);
			}
			dataModel.Name = playerName;
			dataModel.Phone = phone;
			dataModel.Email = emailAddr;
			dataModel.Index = 3;

			SetMethodSelection(3, false);
			nameInputField?.SetTextWithoutNotify(playerName);
			phoneInputField?.SetTextWithoutNotify(phone);
			emailInputField?.SetTextWithoutNotify(emailAddr);
		}

		private void SetMethodSelection(
			int selectedIndex, bool clearInactiveFields)
		{
			BindUi();
			index = Mathf.Clamp(selectedIndex, 1, 3);
			MahjongProductAnalytics.Track("f2_receiver_method_selected", "index", index, "clear_inactive_fields", clearInactiveFields);
			toggleA?.SetIsOnWithoutNotify(index == 1);
			toggleB?.SetIsOnWithoutNotify(index == 2);
			toggleC?.SetIsOnWithoutNotify(index == 3);

			bool isEmail = index == 3;
			email?.SetActive(isEmail);
			phoneFront?.SetActive(!isEmail);
			phoneBehind?.SetActive(!isEmail);
			ApplyRegionalInputPresentation();
			if (!clearInactiveFields)
			{
				return;
			}

			if (isEmail)
			{
				phoneInputField?.SetTextWithoutNotify(string.Empty);
				phone = string.Empty;
			}
			else
			{
				emailInputField?.SetTextWithoutNotify(string.Empty);
				emailAddr = string.Empty;
			}
		}

		private void BindUi()
		{
			if (uiBound)
			{
				return;
			}

			toggleA = FindComponent<Toggle>(
				"PanelA/PanelB/ToggleA");
			toggleB = FindComponent<Toggle>(
				"PanelA/PanelB/ToggleB");
			toggleC = FindComponent<Toggle>(
				"PanelA/PanelB/ToggleC");
			email = FindObject("PanelA/PanelC/Email");
			phoneFront = FindObject(
				"PanelA/PanelC/phoneFront");
			phoneBehind = FindObject(
				"PanelA/PanelC/phoneBehind");
			nameObj = FindObject("PanelA/PanelC/Name");
			toggleGroup = FindComponent<ToggleGroup>(
				"PanelA/PanelB");
			emailInputField = FindComponent<InputField>(
				"PanelA/PanelC/Email");
			phoneInputField = FindComponent<InputField>(
				"PanelA/PanelC/phoneBehind");
			nameInputField = FindComponent<InputField>(
				"PanelA/PanelC/Name");
			brazilPixControls = FindObject(
				"PanelA/PanelC/BrazilPixControls");
			pixCpfCnpjToggle = FindComponent<Toggle>(
				"PanelA/PanelC/BrazilPixControls/CpfCnpj");
			pixPhoneToggle = FindComponent<Toggle>(
				"PanelA/PanelC/BrazilPixControls/Phone");
			pixEmailToggle = FindComponent<Toggle>(
				"PanelA/PanelC/BrazilPixControls/Email");
			pixRandomToggle = FindComponent<Toggle>(
				"PanelA/PanelC/BrazilPixControls/Random");
			pixKeyErrorText = FindComponent<Text>(
				"PanelA/PanelC/BrazilPixControls/Error");
			CaptureDefaultInputPresentation();
			panelE = FindObject("PanelE");

			BindToggle(
				toggleA, OnToggleValueChangeA, false);
			BindToggle(
				toggleB, OnToggleValueChangeB, false);
			BindToggle(
				toggleC, OnToggleValueChangeC, true);
			BindToggle(
				pixCpfCnpjToggle,
				OnPixCpfCnpjToggleChanged,
				true);
			BindToggle(
				pixPhoneToggle,
				OnPixPhoneToggleChanged,
				true);
			BindToggle(
				pixEmailToggle,
				OnPixEmailToggleChanged,
				true);
			BindToggle(
				pixRandomToggle,
				OnPixRandomToggleChanged,
				true);

			if (emailInputField != null)
			{
				emailInputField.onValueChanged.RemoveListener(
					OnInputValueChangeEmail);
				emailInputField.onValueChanged.AddListener(
					OnInputValueChangeEmail);
				emailInputField.onValidateInput -=
					ValidateReceiverCharacter;
				emailInputField.onValidateInput +=
					ValidateReceiverCharacter;
			}
			if (phoneInputField != null)
			{
				phoneInputField.onValueChanged.RemoveListener(
					OnInputValueChangePhone);
				phoneInputField.onValueChanged.AddListener(
					OnInputValueChangePhone);
			}
			if (nameInputField != null)
			{
				nameInputField.onValueChanged.RemoveListener(
					OnInputValueChangeName);
				nameInputField.onValueChanged.AddListener(
					OnInputValueChangeName);
			}

			confirmButton = FindComponent<Button>(
				"PanelA/Confirm");
			BindButton(confirmButton, OnButtonClick);
			closeButton = FindComponent<Button>(
				"PanelA/close");
			BindButton(closeButton, OnCloseBtnClick);
			Button questionButton = FindComponent<Button>(
				"PanelA/Panel/ques");
			BindButton(questionButton, OnButtonClickQues);

			uiBound = email != null &&
				phoneFront != null &&
				phoneBehind != null;
			if (!uiBound)
			{
				Debug.LogError(
					"WithDrawMethod UI binding failed: " +
					"PanelA/PanelC/Email, phoneFront or " +
					"phoneBehind is missing.");
			}
		}

		private void CaptureDefaultInputPresentation()
		{
			if (defaultInputPresentationCaptured ||
				emailInputField == null)
			{
				return;
			}

			defaultEmailContentType =
				emailInputField.contentType;
			defaultEmailInputType =
				emailInputField.inputType;
			defaultEmailKeyboardType =
				emailInputField.keyboardType;
			defaultEmailCharacterValidation =
				emailInputField.characterValidation;
			defaultEmailCharacterLimit =
				emailInputField.characterLimit;
			defaultNameActive = nameObj != null &&
				nameObj.activeSelf;
			defaultInputPresentationCaptured = true;
		}

		private void ApplyRegionalInputPresentation()
		{
			bool isBrazilian = IsBrazilianPaymentMethod();
			if (nameObj != null)
			{
				nameObj.SetActive(
					!isBrazilian && defaultNameActive);
			}
			brazilPixControls?.SetActive(isBrazilian);
			string inputKey = IsBrazilianPaymentMethod() ? "payment.account.pix"
				: IsPhonePaymentMethod() ? "payment.account.phone" : "payment.account.email";
			if (emailInputField != null && emailInputField.placeholder is Text placeholder)
			{
				var localized = placeholder.GetComponent<LocalizedText>();
				if (localized != null) localized.SetTextId(inputKey);
				else placeholder.text = LocalizationManager.Instance?.GetText(inputKey) ?? inputKey;
			}
			if (!defaultInputPresentationCaptured)
			{
				return;
			}

			if (isBrazilian)
			{
				SetPixKeyType(selectedPixKeyType, false);
				return;
			}
			if (IsPhonePaymentMethod())
			{
				emailInputField.contentType = InputField.ContentType.Standard;
				emailInputField.keyboardType = TouchScreenKeyboardType.PhonePad;
				emailInputField.characterValidation = InputField.CharacterValidation.None;
				ClearPixKeyError();
				return;
			}

			emailInputField.inputType =
				defaultEmailInputType;
			emailInputField.keyboardType =
				defaultEmailKeyboardType;
			emailInputField.characterValidation =
				defaultEmailCharacterValidation;
			emailInputField.characterLimit =
				defaultEmailCharacterLimit;
			emailInputField.contentType =
				defaultEmailContentType;
			ClearPixKeyError();
		}

		private bool IsBrazilianPaymentMethod()
		{
			return Framework2Host.PaymentChannel?.InputKind == WaterSort.WaterSortAccountInputKind.Pix;
		}

		private bool IsPhonePaymentMethod() =>
			Framework2Host.PaymentChannel?.InputKind == WaterSort.WaterSortAccountInputKind.Phone;

		private void OnPixCpfCnpjToggleChanged(bool value)
		{
			if (value)
			{
				SetPixKeyType(PixKeyType.CpfCnpj, true);
			}
		}

		private void OnPixPhoneToggleChanged(bool value)
		{
			if (value)
			{
				SetPixKeyType(PixKeyType.Phone, true);
			}
		}

		private void OnPixEmailToggleChanged(bool value)
		{
			if (value)
			{
				SetPixKeyType(PixKeyType.Email, true);
			}
		}

		private void OnPixRandomToggleChanged(bool value)
		{
			if (value)
			{
				SetPixKeyType(PixKeyType.Random, true);
			}
		}

		private void SetPixKeyType(
			PixKeyType keyType, bool clearInput)
		{
			selectedPixKeyType = keyType;
			pixCpfCnpjToggle?.SetIsOnWithoutNotify(
				keyType == PixKeyType.CpfCnpj);
			pixPhoneToggle?.SetIsOnWithoutNotify(
				keyType == PixKeyType.Phone);
			pixEmailToggle?.SetIsOnWithoutNotify(
				keyType == PixKeyType.Email);
			pixRandomToggle?.SetIsOnWithoutNotify(
				keyType == PixKeyType.Random);

			ConfigurePixInputField();
			if (clearInput)
			{
				emailAddr = string.Empty;
				emailInputField?.SetTextWithoutNotify(
					string.Empty);
			}
			ClearPixKeyError();
		}

		private void ConfigurePixInputField()
		{
			if (emailInputField == null)
			{
				return;
			}

			emailInputField.inputType =
				InputField.InputType.Standard;
			emailInputField.keyboardType =
				TouchScreenKeyboardType.Default;
			emailInputField.characterValidation =
				InputField.CharacterValidation.None;
			emailInputField.characterLimit = 0;
			emailInputField.contentType =
				InputField.ContentType.Standard;
		}

		private void ShowPixKeyError(string message)
		{
			if (pixKeyErrorText == null)
			{
				return;
			}
			pixKeyErrorText.text = message ?? string.Empty;
			pixKeyErrorText.gameObject.SetActive(
				!string.IsNullOrEmpty(message));
		}

		private void ClearPixKeyError()
		{
			ShowPixKeyError(string.Empty);
		}

		private void BindToggle(
			Toggle target,
			UnityEngine.Events.UnityAction<bool> callback,
			bool interactable)
		{
			if (target == null)
			{
				return;
			}
			target.onValueChanged.RemoveListener(callback);
			target.onValueChanged.AddListener(callback);
			target.interactable = interactable;
		}

		private void BindButton(
			Button target,
			UnityEngine.Events.UnityAction callback)
		{
			if (target == null)
			{
				return;
			}
			target.onClick.RemoveListener(callback);
			target.onClick.AddListener(callback);
		}

		private GameObject FindObject(string path)
		{
			Transform target = transform.Find(path);
			if (target == null)
			{
				Debug.LogError(
					"WithDrawMethod missing UI path: " +
					path);
				return null;
			}
			return target.gameObject;
		}

		private T FindComponent<T>(string path)
			where T : Component
		{
			GameObject target = FindObject(path);
			T component = target != null
				? target.GetComponent<T>()
				: null;
			if (target != null && component == null)
			{
				Debug.LogError(
					"WithDrawMethod missing component " +
					typeof(T).Name + " at " + path);
			}
			return component;
		}

		public void OnToggleValueChangeA(bool value)
		{
			if (value)
			{
				SetMethodSelection(3, false);
			}
		}

		public void OnToggleValueChangeB(bool value)
		{
			if (value)
			{
				SetMethodSelection(3, false);
			}
		}

		public void OnToggleValueChangeC(bool value)
		{
			if (value)
			{
				SetMethodSelection(3, true);
			}
		}

		public void OnInputValueChangeName(string value)
		{
			playerName = value;
		}

		public void OnInputValueChangePhone(string value)
		{
			phone = value;
		}

		public void OnInputValueChangeEmail(string value)
		{
			string sanitized = SanitizeReceiverInput(value);
			if (emailInputField != null &&
				emailInputField.text != sanitized)
			{
				emailInputField.SetTextWithoutNotify(
					sanitized);
			}
			emailAddr = sanitized;
			if (IsBrazilianPaymentMethod())
			{
				ClearPixKeyError();
			}
		}

		public void OnButtonClick()
		{
			Framework2BusinessAnalytics.Withdraw("receiver_confirm_click", dataModel);
			bool isBrazilian = IsBrazilianPaymentMethod();
			string normalizedReceiver;
			if (!TryNormalizeReceiverInput(
				emailAddr, out normalizedReceiver))
			{
				Framework2BusinessAnalytics.Withdraw("receiver_validation", dataModel, "invalid");
				string errorMessage = isBrazilian
					? LocalizationManager.Instance.GetText("payment.account.pix")
					: LocalizationManager.Instance.GetText("payment.account.invalid");
				if (isBrazilian)
				{
					ShowPixKeyError(errorMessage);
				}
				else if (panelE != null)
				{
					panelE.SetActive(true);
					StartCoroutine(DelayedAction());
				}
				Debug.Log(errorMessage);
				return;
			}

			if (isBrazilian)
			{
				ClearPixKeyError();
			}

			emailAddr = normalizedReceiver;
			if (emailInputField != null &&
				emailInputField.text != emailAddr)
			{
				emailInputField.SetTextWithoutNotify(
					emailAddr);
			}
			if (dataModel == null)
			{
				return;
			}
			dataModel.Name = playerName;
			dataModel.Email = emailAddr;
			dataModel.Phone = string.Empty;
			dataModel.Index = 3;
			SaveEmail(emailAddr);
			Framework2BusinessAnalytics.Withdraw("receiver_validation", dataModel, "saved");

			if (ShouldSubmitRetryWithdrawOnExit())
			{
				SubmitRetryWithdrawAndClose();
				return;
			}
			if (ShouldOpenProcessViewOnMethodExit())
			{
				GameApp.viewManager?.Open(
					ViewType.WithDrawProcess1,
					dataModel);
			}
			else
			{
				GameApp.viewManager?.Open(
					ViewType.WithDrawConfirm,
					dataModel);
			}
			GameApp.viewManager?.Close(ViewId);
		}

		private IEnumerator DelayedAction()
		{
			yield return new WaitForSeconds(2f);
			panelE?.SetActive(false);
		}

		public void OnButtonClickQues()
		{
			GameApp.viewManager?.Open(ViewType.WithDrawQues);
		}

		public void OnCloseBtnClick()
		{
			Framework2BusinessAnalytics.Withdraw("receiver_close", dataModel, ShouldSubmitRetryWithdrawOnExit() ? "submit_retry" : ShouldOpenProcessViewOnMethodExit() ? "open_process" : ShouldCloseWithoutAdvance() ? "close_only" : "resume_level");
			if (ShouldSubmitRetryWithdrawOnExit())
			{
				SyncEmailForRetrySubmit(false);
				SubmitRetryWithdrawAndClose();
				return;
			}
			if (ShouldOpenProcessViewOnMethodExit())
			{
				SyncEmailForProcessView();
				GameApp.viewManager?.Open(
					ViewType.WithDrawProcess1,
					dataModel);
				GameApp.viewManager?.Close(ViewId);
				return;
			}
			if (ShouldCloseWithoutAdvance())
			{
				GameApp.viewManager?.Close(ViewId);
				return;
			}
			GameManagerWZ.instance.StartSourceFlow(DelayedNextLevel());
		}

		private bool ShouldCloseWithoutAdvance()
		{
			if (dataModel != null &&
				!dataModel.CloseWithoutAdvance &&
				dataModel.Special !=
					DataModel.SpecialLuckyWalletWithdraw &&
				dataModel.Special !=
					DataModel.SpecialWithdrawMissionBindEmail &&
				!dataModel.RetryWithdraw)
			{
				return GameManagerWZ.instance == null;
			}
			return true;
		}

		private bool ShouldOpenProcessViewOnMethodExit()
		{
			return dataModel != null &&
				dataModel.UseProcessViewOnMethodExit;
		}

		private bool ShouldSubmitRetryWithdrawOnExit()
		{
			return dataModel != null &&
				dataModel.RetryWithdraw &&
				dataModel.SubmitRetryWithdrawOnMethodExit;
		}

		private void SyncEmailForProcessView()
		{
			if (dataModel == null ||
				GameManagerWZ.instance == null)
			{
				return;
			}

			string normalizedReceiver;
			if (TryNormalizeReceiverInput(
				emailAddr, out normalizedReceiver))
			{
				ApplyEmailToDataModel(
					normalizedReceiver, false);
				return;
			}

			string fallback =
				!string.IsNullOrEmpty(dataModel.Email)
					? dataModel.Email
					: GameManagerWZ.instance
						.getPlayerEmail();
			dataModel.Email = fallback ?? string.Empty;
			dataModel.Phone = string.Empty;
			dataModel.Index = 3;
		}

		private bool SyncEmailForRetrySubmit(
			bool requireValidInput)
		{
			if (dataModel == null)
			{
				return false;
			}

			string normalizedReceiver;
			if (TryNormalizeReceiverInput(
				emailAddr, out normalizedReceiver))
			{
				ApplyEmailToDataModel(
					normalizedReceiver, true);
				return true;
			}
			if (requireValidInput)
			{
				return false;
			}

			string fallback =
				!string.IsNullOrEmpty(dataModel.Email)
					? dataModel.Email
					: GameManagerWZ.instance != null
						? GameManagerWZ.instance
							.getPlayerEmail()
						: string.Empty;
			if (!TryNormalizeReceiverInput(
				fallback, out normalizedReceiver))
			{
				return false;
			}
			ApplyEmailToDataModel(normalizedReceiver, true);
			return true;
		}

		private void SubmitRetryWithdrawAndClose()
		{
			if (isRetryWithdrawSubmitting)
			{
				return;
			}

			if (SyncEmailForRetrySubmit(false))
			{
				ClientWithdrawRequest request =
					BuildRetryWithdrawRequest();
				if (request != null)
				{
					isRetryWithdrawSubmitting = true;
					SetSubmitButtonsInteractable(false);
					dataModel.UseProcessViewOnMethodExit =
						false;
					dataModel
						.SubmitRetryWithdrawOnMethodExit =
						false;
					IEnumerator routine =
						WithdrawApiService.SubmitWithdraw(
							request,
							OnRetryWithdrawSubmitSuccess,
							OnRetryWithdrawSubmitError);
					if (GameManagerWZ.instance != null)
					{
						GameManagerWZ.instance
							.StartCoroutine(routine);
					}
					else
					{
						StartCoroutine(routine);
					}
				}
			}
			GameApp.viewManager?.Close(ViewId);
		}

		private ClientWithdrawRequest
			BuildRetryWithdrawRequest()
		{
			if (dataModel == null ||
				string.IsNullOrEmpty(dataModel.OrderNo))
			{
				return null;
			}
			string receiver =
				!string.IsNullOrEmpty(dataModel.Email)
					? dataModel.Email
					: dataModel.Receiver;
			if (string.IsNullOrEmpty(receiver))
			{
				return null;
			}

			string withdrawType =
				ResolveRetryWithdrawType();
			return new ClientWithdrawRequest
			{
				OrderNo = dataModel.OrderNo,
				WithdrawType = withdrawType,
				Receiver = receiver,
				Amount = dataModel.Money > 0.0
					? Math.Round(
						dataModel.Money,
						2,
						MidpointRounding.ToEven)
					: (double?)null,
				Currency =
					WithdrawConstants.CurrencyUsd,
				Note = string.Equals(
					withdrawType,
					WithdrawConstants.WithdrawTypeWallet,
					StringComparison.OrdinalIgnoreCase)
						? "Wallet withdraw"
						: "First ad withdraw",
				EmailSubject = string.Empty,
				RecipientType =
					WithdrawConstants.GetCurrentRecipientType()
			};
		}

		private string ResolveRetryWithdrawType()
		{
			if (dataModel != null &&
				!string.IsNullOrEmpty(
					dataModel.WithdrawType))
			{
				return dataModel.WithdrawType;
			}
			return dataModel != null &&
				dataModel.Special ==
					DataModel.SpecialLuckyWalletWithdraw
				? WithdrawConstants.WithdrawTypeWallet
				: WithdrawConstants.WithdrawTypeFirstAd;
		}

		private void OnRetryWithdrawSubmitSuccess(
			WithdrawOrderData order)
		{
			isRetryWithdrawSubmitting = false;
			SetSubmitButtonsInteractable(true);
			if (order == null || dataModel == null)
			{
				return;
			}
			dataModel.OrderNo = order.OrderNo;
			dataModel.WithdrawType = order.WithdrawType;
			dataModel.OrderStatus = order.OrderStatus;
			dataModel.Receiver = order.Receiver;
			dataModel.WalletFrozen = order.WalletFrozen;
			Framework2BusinessAnalytics.Withdraw("receiver_retry_result", dataModel, "stored_locally");
		}

		private void OnRetryWithdrawSubmitError(string error)
		{
			Framework2BusinessAnalytics.Withdraw("receiver_retry_result", dataModel, "failed");
			isRetryWithdrawSubmitting = false;
			SetSubmitButtonsInteractable(true);
			if (string.IsNullOrEmpty(error))
			{
				return;
			}
			Debug.LogWarning(
				"Retry withdraw request failed: " + error);
			WithdrawApiService.LogPaymentNotice(
				"WithDrawMethod",
				error,
				LogType.Warning);
		}

		private void SetSubmitButtonsInteractable(
			bool interactable)
		{
			if (confirmButton != null)
			{
				confirmButton.interactable = interactable;
			}
			if (closeButton != null)
			{
				closeButton.interactable = interactable;
			}
		}

		private char ValidateReceiverCharacter(
			string text, int charIndex, char addedChar)
		{
			if (IsPhonePaymentMethod()) return char.IsDigit(addedChar) ||
				(addedChar == '+' && charIndex == 0) ? addedChar : '\0';
			if (IsBrazilianPaymentMethod())
			{
				return addedChar;
			}
			return char.IsWhiteSpace(addedChar) ||
				!IsAllowedEmailCharacter(addedChar)
				? '\0'
				: addedChar;
		}

		private string SanitizeReceiverInput(string value)
		{
			if (IsPhonePaymentMethod()) return (value ?? string.Empty).Trim();
			if (IsBrazilianPaymentMethod())
			{
				return value ?? string.Empty;
			}
			return SanitizeEmailInput(value);
		}

		private bool TryNormalizeReceiverInput(
			string value, out string normalized)
		{
			var channel = Framework2Host.PaymentChannel;
			if (channel != null && !string.IsNullOrEmpty(channel.AccountPattern))
			{
				normalized = SanitizeReceiverInput(value).Trim();
				try { return Regex.IsMatch(normalized, channel.AccountPattern); }
				catch (ArgumentException) { return false; }
			}
			if (IsBrazilianPaymentMethod())
			{
				normalized = value ?? string.Empty;
				return !string.IsNullOrWhiteSpace(normalized);
			}
			normalized = SanitizeEmailInput(value);
			return IsValidComEmail(normalized);
		}

		private string SanitizeEmailInput(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}
			char[] buffer = new char[value.Length];
			int length = 0;
			for (int i = 0; i < value.Length; i++)
			{
				char character = value[i];
				if (IsAllowedEmailCharacter(character))
				{
					buffer[length++] = character;
				}
			}
			return new string(buffer, 0, length).Trim();
		}

		private bool IsAllowedEmailCharacter(char value)
		{
			return char.IsLetterOrDigit(value) ||
				value == '%' ||
				value == '+' ||
				value == '-' ||
				value == '.' ||
				value == '@' ||
				value == '_';
		}

		private bool IsValidComEmail(string value)
		{
			return !string.IsNullOrEmpty(value) &&
				EmailRegex != null &&
				EmailRegex.IsMatch(value);
		}

		private IEnumerator DelayedNextLevel()
		{
			yield return new WaitForSeconds(0.2f);
			if (WithdrawalTaskPresentation.ShouldResumeCompletedLevel(GameManagerWZ.instance, dataModel))
			{
				GameManagerWZ.instance.NextLevel();
			}
			GameApp.viewManager?.Close(ViewId);
		}

		private void ApplyEmailToDataModel(
			string value, bool updateReceiver)
		{
			emailAddr = value;
			dataModel.Email = value;
			if (updateReceiver)
			{
				dataModel.Receiver = value;
			}
			dataModel.Phone = string.Empty;
			dataModel.Index = 3;
			SaveEmail(value);
			if (emailInputField != null &&
				emailInputField.text != value)
			{
				emailInputField.SetTextWithoutNotify(value);
			}
		}

		private void SaveEmail(string value)
		{
			if (GameManagerWZ.instance == null)
			{
				return;
			}
			GameManagerWZ.instance.SavePlayerEmail(value);
			GameManagerWZ.instance.SavePlayerIndex(3);
		}

	}
}
