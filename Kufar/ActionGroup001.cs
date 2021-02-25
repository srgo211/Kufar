using System;
using System.Data;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Resources;
using System.ComponentModel;
using System.Collections.Generic;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Enums;
using ZennoLab.Emulation;
using Global.ZennoExtensions;
using ZennoLab.CommandCenter.TouchEvents;
using ZennoLab.CommandCenter.FullEmulation;
using ZennoLab.InterfacesLibrary.Enums;

namespace Kufar
{
	/// <summary>
	/// Класс выполнения группы действий
	/// </summary>
	internal class ActionGroup001
	{
		/// <summary>
		/// Метод выполнения группы действий
		/// </summary>
		/// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
		/// <param name="project">Объект проекта выделеный для данного скрипта</param>
		/// <returns>Код выполнения группы действий</returns>
		public static int Execute(Instance instance, IZennoPosterProjectModel project)
		{
			instance.ClearCookie();

			Tab tab = instance.ActiveTab;
			if ((tab.IsVoid) || (tab.IsNull)) return -1;
			if (tab.IsBusy) tab.WaitDownloading();
			tab.Navigate("https://www.kufar.by/listings", "");
			if (tab.IsBusy) tab.WaitDownloading();


			return 0;
		}
	}
}