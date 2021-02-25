using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;

namespace Kufar
{
	public static class ZennoExstensions
	{
		/// <summary>
		/// Переход по URL и Ожидание загрузки страницы
		/// </summary>
		/// <param name="tab"></param>
		/// <param name="url">ссылка</param>
		/// <param name="pause">пауза после прогрузки страницы</param>
		/// <param name="referer">referer</param>
		/// <returns></returns>
		public static Tab NavigateAndWait(this Tab tab, string url, int pause = 1000, string referer = "")
		{
			tab.Navigate(url, referer);
			if (tab.IsBusy)
			{
				tab.WaitDownloading();
			}
			System.Threading.Thread.Sleep(pause);
			return tab;
		}

		/// <summary>
		/// Ожидание загрузки страница и пауза
		/// </summary>
		/// <param name="tab"></param>
		/// <param name="pause">пауза</param>
		/// <returns></returns>
		public static Tab WaitDownloading(this Tab tab, int pause)
		{
			if (tab.IsBusy)
			{
				tab.WaitDownloading();
			}
			System.Threading.Thread.Sleep(pause);
			return tab;
		}

		/// <summary>
		/// Поиск HtmlElement по XPath
		/// </summary>
		/// <param name="tab"></param>
		/// <param name="xpath">путь XPath</param>
		/// <param name="number">номер елемента (по умл. 0)</param>
		/// <returns>HtmlElement</returns>
		public static HtmlElement FindElementByXpath(this Tab tab, string xpath, int number = 0)
		{
			return tab.FindElementByXPath(xpath, number);
		}

		/// <summary>
		/// Ожидание загрузки элемента
		/// </summary>
		/// <param name="tab"></param>
		/// <param name="xpath">путь</param>
		/// <param name="number">номер</param>
		/// <param name="timeout">время ожидания мс.</param>
		/// <param name="exceptionMessage">текст исключения в случае ошибки</param>
		/// <returns></returns>
		public static HtmlElement WaitElement(this Tab tab, string xpath, int timeout = 5000, int number = 0, string exceptionMessage = "HtmlElement не найден")
		{
			HtmlElement el = tab.FindElementByXPath(xpath, number);
			for (int l = 0; l < timeout / 100; l++)
			{
				if (!el.IsVoid) return el;
				System.Threading.Thread.Sleep(100);
			}
			throw new Exception(exceptionMessage);
		}

		/// <summary>
		/// Поиск HtmlElement за указанное время (true-найден; false - нет)
		/// </summary>
		/// <param name="tab"></param>
		/// <param name="xpath">Xpath путь</param>
		/// <param name="number">номер (по умл. 0)</param>
		/// <param name="timeout">поиск в течении мс. (по умл. 5000)</param>
		/// <returns></returns>
		public static bool CheckHtmlElement(this Tab tab, string xpath, int timeout = 5000, int number = 0)
		{
			HtmlElement el = tab.FindElementByXPath(xpath, number);
			for (int l = 0; l < timeout / 100; l++)
			{
				if (!el.IsVoid) return true;
				System.Threading.Thread.Sleep(100);
			}
			return false;
		}


		/// <summary>
		/// Поиск HtmlElement за указанное время
		/// </summary>
		/// <param name="tab"></param>
		/// <param name="xpath">путь xPath до елемента</param>
		/// <param name="timeout">время ожидания элемента в мс.(по умл. 5000)</param>
		/// <param name="number">номер пути xPath (по умл. 0)</param>
		/// <param name="errMsg">если не найден текст ошибки</param>
		/// <returns></returns>
		public static Tab WaitHtmlElement(this Tab tab, string xpath, int timeout = 5000, int number = 0, string errMsg = "Не нашли элемент")
		{
			HtmlElement el = tab.FindElementByXPath(xpath, number);
			for (int l = 0; l < timeout / 100; l++)
			{
				if (!el.IsVoid) return tab;
				System.Threading.Thread.Sleep(100);
			}
			throw new Exception(errMsg);
		}

		/// <summary>
		/// Клик по элементу
		/// </summary>
		/// <param name="tab"></param>
		/// <param name="xpath">путь Xpath</param>
		/// <param name="number">пауза в мс.</param>
		/// <returns></returns>
		public static Tab Click(this Tab tab, string xpath, int pause = 1000, int number = 0)
		{
			tab.FindElementByXPath(xpath, number).Click();
			tab.WaitDownloading();
			System.Threading.Thread.Sleep(pause);
			return tab;
		}




		/// <summary>
		/// Поиск и ожидание загрузки HtmlElementa
		/// </summary>
		/// <param name="tab"></param>
		/// <param name="xpath">путь XPath</param>
		/// <param name="number">номер XPath (по умл. 0)</param>
		/// <param name="timeout">время ожидания мс (по умл 3000)</param>
		/// <param name="exceptionMessage">если не найден элемент, то текст ошибки</param>
		/// <returns>HtmlElement</returns>
		public static HtmlElement FindElementByXpathAndWait(this Tab tab, string xpath, int timeout = 3000, int number = 0, string exceptionMessage = "HtmlElement не найден")
		{
			HtmlElement el = tab.FindElementByXPath(xpath, number);

			for (int l = 0; l < timeout / 100; l++)
			{
				if (!el.IsVoid) return el;
				System.Threading.Thread.Sleep(100);
			}
			throw new Exception(exceptionMessage);

		}



		/// <summary>
		///Вставка текста 
		/// </summary>
		/// <param name="tab">имя  вкладки</param>
		/// <param name="text">текст для вставки</param>
		/// <param name="XPath">путь XPath до элемента</param>
		/// <param name="number">номер пути XPath (по умолчанию 0)</param>
		/// <param name="emulation">уровень эмуляции -"Full","Middle","None" (по умолчанию Full)</param>
		/// <param name="pause">пауза после вставки текста мс.(по умолчанию 1000)</param>
		public static Tab TextSetValue(this Tab tab, string text, string XPath, int pause = 1000, int number = 0, string emulation = "Full")
		{

			HtmlElement he = tab.FindElementByXPath(XPath, number);
			he.SetValue(text, emulation);
			tab.WaitDownloading();
			System.Threading.Thread.Sleep(pause);
			return tab;

		}





		/// <summary>
		/// Обычный Ввод текста
		/// </summary>
		/// <param name="instance">текущий instance</param>
		/// <param name="text">текст для ввода</param>
		/// <param name="XPath">путь XPath до элемента</param>
		/// <param name="number">номер пути XPath (по умолчанию 0)</param>
		/// <param name="emulation">уровень эмуляция (по умл. Full)</param>
		/// <param name="pauseMin">задержка ввода символов ОТ</param>
		/// <param name="pauseMax">задержка ввода символов ДО</param>
		/// <param name="symboText">по умл. true -вводим посимвольно; false - стандартным способом зенки</param>
		public static Tab InputText(Instance instance, string text, string XPath, int number = 0, string emulation = "Full", int pauseMin = 15, int pauseMax = 65, bool symboText = true)
		{
			Random rnd = new Random();
			HtmlElement he = instance.ActiveTab.FindElementByXPath(XPath, number);
			he.RiseEvent("click", emulation);
			System.Threading.Thread.Sleep(200);


			if (symboText)
			{
				text = text.Replace("\r\n", "⋇");
				foreach (var symbol in text)
				{
					if (symbol == '⋇')
						instance.SendText("{ENTER}", rnd.Next(pauseMin, pauseMax));
					else
					{
						instance.SendText(symbol.ToString(), rnd.Next(pauseMin, pauseMax));
					}
				}
			}
			else
			{
				instance.SendText(text, rnd.Next(pauseMin, pauseMax));
			}

			return instance.ActiveTab;
		}


	}
}
