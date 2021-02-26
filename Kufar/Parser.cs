using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Kufar
{
    internal class Parser
    {
        public const string connectBD = "Server=localhost;Database=kufar;Uid=root;Pwd=vertrigo;Port=3306;CharSet=utf8;Pooling=True;";
        public const string baseUrl = "https://www.kufar.by/listings?rgn=4&ar=12";

        /// <summary>
        /// Метод выполнения группы действий
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="project">Объект проекта выделеный для данного скрипта</param>
        /// <returns>Код выполнения группы действий</returns>
        public static int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            string userAgent = project.Profile.UserAgent;
            string url = "https://www.kufar.by/listings?rgn=4&ar=12";
            url = "https://www.kufar.by/listings?rgn=4&ar=12&cursor=eyJ0IjoiYWJzIiwiZiI6ZmFsc2UsInAiOjF9";

            string res = string.Empty;
            using (var request = new HttpRequest())
            {
                request.UserAgent = userAgent;

                // Отправляем запрос.
                HttpResponse response = request.Get(url);

                // Принимаем тело сообщения в виде строки.
                res = response.ToString();


            }

            //(?<=type="application/json">)[\w\W]*?(?=</script>)
            string jsonResult = new Regex("(?<=type=\"application/json\">)[\\w\\W]*?(?=</script>)").Match(res).ToString();

            if (string.IsNullOrEmpty(jsonResult)) return -1;

            dynamic json = JObject.Parse(jsonResult);

            foreach (var j in json?.props?.initialState?.listingGeneralist?.listingElements)
            {


                DataParse data = new DataParse()
                {
                    Id = j.id,
                    CategoryName = j.categoryName,
                    Idcategory = j.category,
                    IdParentCategory = j.parent,
                    Link = j.adViewLink,
                    Price = j.price.ru,
                    Title = j.title,
                    UserName = j.userName

                };

                Send.InfoToLog(project, $"Id - {data.Id}\n" +
                                        $"CategoryName - {data.CategoryName}\n" +
                                        $"Link - {data.Link}\n" +
                                        $"Price - {data.Price}\n" +
                                        $"Title  - {data.Title }+ \n");



            }


            string next = string.Empty;

            foreach (var el in json?.props?.initialState?.listingGeneralist?.pagination)
            {
                if (el.label == "next")
                {
                    next = el.token;
                    break;
                }
            }

            Send.InfoToLog(project, next);

            return 0;
        }


        public static void AvtorizationByProfile(Instance instance, IZennoPosterProjectModel project, string pathProfile)
        {
            project.Profile.Load(pathProfile);

            instance.ActiveTab.NavigateAndWait(baseUrl, 2000);

            //TODO проверка на авторизацию

        }


        public static string GetNomerPhone(Instance instance, IZennoPosterProjectModel project, string idItem, string proxy = null)
        {
            string userAgent = project.Profile.UserAgent;

            ICookieContainer cookieContainer = project.Profile.CookieContainer;
            string url = $"https://cre-api.kufar.by/items-search/v1/engine/v1/item/{idItem}/phone";
            string res = string.Empty;


            const string domen = "kufar.by";
            string cookies = instance.GetCookie(domen, true);

            //string proxy = "45.89.231.240:55762:xHTsepsS:Tb9qBfym";

            CookieStorage cookieStorage = new CookieStorage();



            string[] arrCook = cookies.Split(new string[] { ";" }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var el in arrCook)
            {
                string[] elArr = el.Split('=');
                var cook = new Cookie(elArr[0].Trim(), elArr[1].Trim());
                cook.Domain = domen;
                cookieStorage.Add(cook);

            }






            using (var rq = new HttpRequest())
            {
                rq.UserAgent = userAgent;

                rq.UseCookies = true;

                rq.Cookies = cookieStorage;
                if (!string.IsNullOrWhiteSpace(proxy)) rq.Proxy = ProxyClient.Parse(ProxyType.HTTP, proxy);


                // Отправляем запрос.
                HttpResponse response = rq.Get(url);

                // Принимаем тело сообщения в виде строки.
                res = response.ToString();


            }

            dynamic json = JObject.Parse(res);

            //Send.InfoToLog(project, json.ToSring());

            return json?.phone;
        }

    }
}
