﻿using System.Collections.Generic;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Kufar
{
    interface IWorkingProcedure
    {

        /// <summary>получаем список спаршенных данных</summary>
        List<DataParse> GetListDataParses(Instance instance, IZennoPosterProjectModel project);

        /// <summary>Авторизация аккаунта</summary>
        void AvtorizationAccount(Instance instance, IZennoPosterProjectModel project);

        /// <summary>получаем список данных с номерами телефонов</summary>
        List<DataParse> GetDataAndNomerPhone(Instance instance, IZennoPosterProjectModel project, List<DataParse> dataParses, string proxy = null);

        /// <summary>получаем полную Карточку товара</summary>
        List<DataParse> GetParseDataFromProducts(Instance instance, IZennoPosterProjectModel project, List<DataParse> dataParses);

        /// <summary>
        /// Добавляем в БД данные
        /// </summary>
        /// <param name="dataParses"></param>
        void AddBD(List<DataParse> dataParses);
    }








}