using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace firesafety
{
    public class firesafetyPlugin : Renga.IPlugin
    {
        private List<Renga.ActionEventSource> m_eventSources = new List<Renga.ActionEventSource>();
        private static Renga.Application app = new Renga.Application ();
        private static int doorSpParamHeight = 1900;
        private static int doorSpParamWidth = 800;
        private static double stairSpParamHeight = 2200;
        private static double stairSpParamWidth = 900;
        private static double stairSpSlope = 45;
        private static double emergencyWaySpHeight = 2000;
        private static double emergencyWaySpWidth = 1000;

        public bool Initialize(string pluginFolder)
        {
            //var app = new Renga.Application();
            var ui = app.UI;
            var panelExtension = ui.CreateUIPanelExtension();

            string ButtonToolTip = "Проверка требований пожарной безопасности";

            Renga.IAction[] actions = new Renga.IAction[4];
            actions[0] = CreateAction("Создание свойств объектов для работы плагина", "Действие 1", propertySet);
            actions[1] = CreateAction("Проверка эвакуационных выходов", "Действие 2", buttonMain);
            actions[2] = CreateAction("Проверка параметров лестниц на соответсвие нормам", "Действие 3", buttonSecond);
            actions[3] = CreateAction("Проверка горизонтальных эвакуационных путей", "Действие 4", buttonThird);

            CreateDropDownButton(ButtonToolTip, actions);
         
            return true;
        }
        public void Stop()
        {
            foreach (var eventSource in m_eventSources)
                eventSource.Dispose();

            m_eventSources.Clear();
        }
        public void propertySet()
        {
            Renga.PropertyDescription propertyDescription = new Renga.PropertyDescription();
            propertyDescription.Name = "Эвакуационный выход";
            propertyDescription.Type = Renga.PropertyType.PropertyType_Boolean;

            Guid attributeUuid = Guid.NewGuid();

            Renga.IPropertyManager propertyManager = app.Project.PropertyManager;
            propertyManager.RegisterProperty(attributeUuid, propertyDescription);

            propertyDescription = new Renga.PropertyDescription();
            propertyManager.AssignPropertyToType(attributeUuid, Renga.ObjectTypes.Door);

            propertyDescription.Name = "Эвакуационный путь";
            propertyDescription.Type = Renga.PropertyType.PropertyType_Boolean;

            attributeUuid = Guid.NewGuid();

            propertyManager.RegisterProperty(attributeUuid, propertyDescription);
            propertyManager.AssignPropertyToType(attributeUuid, Renga.ObjectTypes.Room);

            propertyDescription.Name = "Эвакуационная лестница";
            propertyDescription.Type = Renga.PropertyType.PropertyType_Boolean;

            attributeUuid = Guid.NewGuid();

            propertyManager.RegisterProperty(attributeUuid, propertyDescription);
            propertyManager.AssignPropertyToType(attributeUuid, Renga.ObjectTypes.Stair);
        }

            public static void CreateDropDownButton(string toolTip, Renga.IAction[] actions)
        {
            Renga.IUIPanelExtension panelExtension = app.UI.CreateUIPanelExtension();
            Renga.IDropDownButton dropDownButton = app.UI.CreateDropDownButton();

            Renga.IImage image = app.UI.CreateImage();
            //image.LoadFromFile(iconPath);

            dropDownButton.ToolTip = toolTip;
            dropDownButton.Icon = image;

            foreach (var action in actions)
            {
                dropDownButton.AddAction(action);
            }

            panelExtension.AddDropDownButton(dropDownButton);
            app.UI.AddExtensionToPrimaryPanel(panelExtension);
            app.UI.AddExtensionToActionsPanel(panelExtension, Renga.ViewType.ViewType_View3D);

            return;
        }

        public Renga.IAction CreateAction(string displayName, string toolTip, Action handler)
        {
            Renga.IImage image = app.UI.CreateImage();

            Renga.IAction action = app.UI.CreateAction();
            action.DisplayName = displayName;
            action.ToolTip = toolTip;
            action.Icon = image;

            var events = new Renga.ActionEventSource(action);
            events.Triggered += (s, e) =>
            {
                handler();
            };

            m_eventSources.Add(events);

            return action;
        }
        public static void buttonMain()
        {
            plugin form = new plugin();
            string titleMessage = "Проверка эвакуационных выходов,их ширины и высоты на соответсвие нормам, приведенные в СП 1.13130.2020 'СИСТЕМЫ ПРОТИВОПОЖАРНОЙ ЗАЩИТЫ' - 'ЭВАКУАЦИОННЫЕ ПУТИ И ВЫХОДЫ'";
            string resultMessage = "";

            Renga.IModelObjectCollection objects = app.Project.Model.GetObjects();
            Renga.IPropertyManager manager = app.Project.PropertyManager;
            string emergPropertyId = "";
            
            for (int i = 0; i < manager.PropertyCount; i++)
            {
                string propertyId = manager.GetPropertyIdS(i);
                if (manager.GetPropertyNameS(propertyId) == "Эвакуационный выход")
                {
                    emergPropertyId = propertyId;
                    break;
                }
            }
            if (emergPropertyId == "")
            {
                resultMessage += $"Ошибка! В проекте не созданы свойства для объектов!\n";
                resultMessage += $"В меню плагина выберите действие 'Создать свойста', затем задайте свойство 'Эвакуационный выход' - 'Да' для необходмых дверей!\n";

                form.setValue(titleMessage, resultMessage);
                form.ShowDialog();
                return;
            }
            string errorMessage = "";
            List<int> errors = new List<int>();
            for (int i = 0; i < objects.Count; i++)
            {
                errorMessage = "";
                var obj = objects.GetByIndex(i);

                if (obj.ObjectType != Renga.ObjectTypes.Door)
                {
                    continue;
                }
                var doorParams = obj as Renga.IDoorParams;
                Renga.IPropertyContainer properties = obj.GetProperties();
                Renga.IProperty emergProperty = properties.GetS(emergPropertyId);

                if (!emergProperty.GetBooleanValue())
                {
                    continue;
                }
                if (resultMessage == "")
                {
                    resultMessage += "Ширина и высота эвакуационных выходов в проекте:\n";
                }

                if (doorParams.Height < doorSpParamHeight)
                {
                    errorMessage += "Высота не соотвествует нормам! | ";
                }
                if (doorParams.Width < doorSpParamWidth)
                {
                    errorMessage += "Ширина не соотвествует нормам! | ";
                }

                if (errorMessage == "")
                {
                    resultMessage += $"Ширина: {doorParams.Width} мм | Высота: {doorParams.Height} мм - Значения соответствуют нормам!\n\n";
                }
                else
                {
                    resultMessage += $"Ширина: {doorParams.Width} мм | Высота: {doorParams.Height} мм -  {errorMessage}\n\n";
                    errors.Add(obj.Id);
                }

            }

            if (resultMessage == "")
            {
                resultMessage += $"Ошибка! В проекте не найдены эвакуационные выходы!\n";
                resultMessage += $"Задайте свойство 'Эвакуационный выход' - 'Да' для необходмых дверей!\n";
                form.setValue(titleMessage, resultMessage);
                form.ShowDialog();
                return;
            }
            app.Selection.SetSelectedObjects(errors.ToArray());


            form.setValue(titleMessage, resultMessage);
            form.ShowDialog();
        }
        public static void buttonSecond()
        {
            plugin form = new plugin();
            string titleMessage = "Проверка параметров лестниц на соответсвие нормам, приведенные в СП 1.13130.2020 'СИСТЕМЫ ПРОТИВОПОЖАРНОЙ ЗАЩИТЫ' - 'ЭВАКУАЦИОННЫЕ ПУТИ И ВЫХОДЫ'";
            string resultMessage = "";

            Renga.IModelObjectCollection objects = app.Project.Model.GetObjects();
            Renga.IPropertyManager manager = app.Project.PropertyManager;
            string emergPropertyId = "";
            
            for (int i = 0; i < manager.PropertyCount; i++)
            {
                string propertyId = manager.GetPropertyIdS(i);
                if (manager.GetPropertyNameS(propertyId) == "Эвакуационная лестница")
                {
                    emergPropertyId = propertyId;
                    break;
                }
            }
            if (emergPropertyId == "")
            {
                resultMessage += $"Ошибка! В проекте не созданы свойства для объектов!\n";
                resultMessage += $"В меню плагина выберите действие 'Создать свойста', затем задайте свойство 'Эвакуационная лестница' - 'Да' для необходмых лестниц!\n";

                form.setValue(titleMessage, resultMessage);
                form.ShowDialog();
                return;
            }
            string errorMessage = "";
            List<int> errors = new List<int>();
            for (int i = 0; i < objects.Count; i++)
            {
                errorMessage = "";
                var obj = objects.GetByIndex(i);

                if (obj.ObjectType != Renga.ObjectTypes.Stair)
                {
                    continue;
                }
                Renga.IPropertyContainer properties = obj.GetProperties();
                Renga.IProperty emergProperty = properties.GetS(emergPropertyId);

                if (!emergProperty.GetBooleanValue())
                {
                    continue;
                }
                if (resultMessage == "")
                {
                    resultMessage += "Параметры лестниц в проекте:\n";
                }

                Renga.IParameterContainer paramContainer = obj.GetParameters();
                double stairHeight = paramContainer.Get(Renga.ParameterIds.StairHeight).GetDoubleValue();
                double stairWidth = paramContainer.Get(Renga.ParameterIds.StairWidth).GetDoubleValue();
                double stairSlope = paramContainer.Get(Renga.ParameterIds.StairSlope).GetDoubleValue();

                if (stairHeight < stairSpParamHeight)
                {
                    errorMessage += "Высота не соотвествует нормам! | ";
                }
                if (stairWidth < stairSpParamWidth)
                {
                    errorMessage += "Ширина не соотвествует нормам! | ";
                }

                if (stairSlope > stairSpSlope)
                {
                    errorMessage += "Угол наклона не соотвествует нормам! | ";
                }

                if (errorMessage == "")
                {
                    resultMessage += $"Ширина: {stairWidth} мм | Высота: {stairHeight} мм | Угол наклона лестницы: {Math.Round(stairSlope, 2)}°  - Значения соответствуют нормам!\n\n";
                }
                else
                {
                    resultMessage += $"Ширина: {stairWidth} мм | Высота: {stairHeight} мм | Угол наклона лестницы: {Math.Round(stairSlope, 2)}°  -  {errorMessage}\n\n";
                    errors.Add(obj.Id);
                }

            }

            if (resultMessage == "")
            {
                resultMessage += $"Ошибка! В проекте не найдены эвакуационные лестницы!\n";
                resultMessage += $"Задайте свойство 'Эвакуационная лестница' - 'Да' для необходмых лестниц!\n";
                form.setValue(titleMessage, resultMessage);
                form.ShowDialog();
                return;
            }
            app.Selection.SetSelectedObjects(errors.ToArray());


            form.setValue(titleMessage, resultMessage);
            form.ShowDialog();
        }
        public static void buttonThird()
        {
            plugin form = new plugin();
            string titleMessage = "Проверка ширины и высоты горизонтальных эвакуационных путей  на соответсвие нормам, приведенные в СП 1.13130.2020 'СИСТЕМЫ ПРОТИВОПОЖАРНОЙ ЗАЩИТЫ' - 'ЭВАКУАЦИОННЫЕ ПУТИ И ВЫХОДЫ'";
            string resultMessage = "";

            Renga.IModelObjectCollection objects = app.Project.Model.GetObjects();
            Renga.IPropertyManager manager = app.Project.PropertyManager;
            string emergPropertyId = "";

            for (int i = 0; i < manager.PropertyCount; i++)
            {
                string propertyId = manager.GetPropertyIdS(i);
                if (manager.GetPropertyNameS(propertyId) == "Эвакуационный путь")
                {
                    emergPropertyId = propertyId;
                    break;
                }
            }
            if (emergPropertyId == "")
            {
                resultMessage += $"Ошибка! В проекте не созданы свойства для объектов!\n";
                resultMessage += $"В меню плагина выберите действие 'Создать свойста', затем задайте свойство 'Эвакуационный путь' - 'Да' для необходмых помещений!\n";

                form.setValue(titleMessage, resultMessage);
                form.ShowDialog();
                return;
            }
            string errorMessage = "";
            List<int> errors = new List<int>();
            for (int i = 0; i < objects.Count; i++)
            {
                errorMessage = "";
                var obj = objects.GetByIndex(i);

                if (obj.ObjectType != Renga.ObjectTypes.Room)
                {
                    continue;
                }
                Renga.IPropertyContainer properties = obj.GetProperties();
                Renga.IProperty emergProperty = properties.GetS(emergPropertyId);

                if (!emergProperty.GetBooleanValue())
                {
                    continue;
                }
                if (resultMessage == "")
                {
                    resultMessage += "Ширина и высота горизонатльных эвакуационных путей в проекте:\n";
                }

                Renga.IParameterContainer paramContainer = obj.GetParameters();
                double roomHeight = paramContainer.Get(Renga.ParameterIds.RoomHeight).GetDoubleValue();
                Renga.IQuantityContainer roomInfo = obj.GetQuantities();
                double roomPerimeter = roomInfo.Get(Renga.QuantityIds.GrossPerimeter).AsLength(Renga.LengthUnit.LengthUnit_Millimeters);
                double roomArea = roomInfo.Get(Renga.QuantityIds.GrossFloorArea).AsArea(Renga.AreaUnit.AreaUnit_Millimeters2);
                double roomWidth = findWidthRoom(roomPerimeter, roomArea);

                

                if (roomHeight < emergencyWaySpHeight)
                {
                    errorMessage += "Высота не соотвествует нормам! | ";
                }
                if (roomWidth < emergencyWaySpWidth)
                {
                    errorMessage += "Ширина не соотвествует нормам! | ";
                }

                if (errorMessage == "")
                {
                    resultMessage += $"Высота: {roomHeight} мм | Ширина мм - {roomWidth} - Значения соответствуют нормам!\n\n";
                }
                else
                {
                    resultMessage += $"Высота: {roomHeight} мм | Ширина мм - {roomWidth}  -  {errorMessage}\n\n";
                    errors.Add(obj.Id);
                }

            }

            if (resultMessage == "")
            {
                resultMessage += $"Ошибка! В проекте не найдены эвакуационные пути!\n";
                resultMessage += $"Задайте свойство 'Эвакуационный путь' - 'Да' для необходмых помещений!\n";
                form.setValue(titleMessage, resultMessage);
                form.ShowDialog();
                return;
            }
            app.Selection.SetSelectedObjects(errors.ToArray());


            form.setValue(titleMessage, resultMessage);
            form.ShowDialog();
        }

        public static double  findWidthRoom(double perimeter, double area)
        {
            double length = 0;
            double width = 1000;
            double a = 2;
            double b = 2 * area;
            double c = perimeter;
            double discr = Math.Pow(b, 2) - (4 * a * c);
            double x1 = (-b + Math.Sqrt(discr) ) / 2 * a;
            double x2 = (-b - Math.Sqrt(discr)) / 2 * a;
            if (x1 > 0) {
                length = x1;
            }
            else
            {
                length = x2;
            }
            return width;
        }

    }
}
