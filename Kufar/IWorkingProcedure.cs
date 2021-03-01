using System.Collections.Generic;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Kufar
{
    interface IWorkingProcedure
    {


        /// <summary>получаем список спаршенных данных</summary>
        /// <param name="counParsePage">кол-во страниц для парсинга (-1: парсить все)</param>
        void GetListDataParses(Instance instance, IZennoPosterProjectModel project, int counParsePage);

        /// <summary>Авторизация аккаунта</summary>
        void AvtorizationAccount(Instance instance, IZennoPosterProjectModel project, string pathProfile, string proxy);

        /// <summary>получаем список данных с номерами телефонов</summary>
        List<DataParse> GetDataAndNomerPhone(Instance instance, IZennoPosterProjectModel project, List<DataParse> dataParses, string proxy = null);

        List<DataParse> GetDataAndNomerPhone(Instance instance, IZennoPosterProjectModel project, int IdFromBD, string proxy = null);

        /// <summary>получаем полную Карточку товара</summary>
        void GetParseDataFromProducts(Instance instance, IZennoPosterProjectModel project);

        /// <summary>
        /// Добавляем в БД данные
        /// </summary>
        /// <param name="dataParses"></param>
        void AddBD(List<DataParse> dataParses);
    }






}
