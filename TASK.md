# Доработка Diagram Designer

Статья о создании Diagram Designer и его исходный код, с которого начинаем работу:
<https://www.codeproject.com/Articles/24681/WPF-Diagram-Designer-Part-4>

**Требования:** минимальный Framework не выше 4.0. Совместимость с Visual Studio
2015/2017, комментирование всех измененных и добавленных в исходниках мест (чтобы
можно было отличить ваш код от изначального).  

В описании задачи используются следующие термины:  

*Блок* - фигура, которую можно перетащить из Toolbox на рабочее поле диаграммы.  
*Связь* - это линия, которая соединяет два блока, вместе с её точками начала и конца, а
также стрелочкой.  
*Точка привязки блока* - точка на блоке, из которой может выходить или в которую может
приходить связь.  
*Диаграмма* - совокупность блоков и связей между ними, которая может быть отображена
на экране или сохранена в один файл.  
*Конфигурация типа блока* - запись в формате XML, которая содержит описание внешнего
вида блока определенного типа, а также некоторые его свойства. Сейчас все
конфигурации типов хранятся в ресурсах приложения.  

## Задачи

1. **Изменить поведение стандартных блоков и связей между ними**
   1. **(?)** Выделение связи по клику на любую точку линии связи (сейчас связи
выделяются только если кликнуть на их точку начала или конца)
   1. Удаление связи по нажатию на клавишу Del клавиатуры (сейчас удаляется
только по нажатию на кнопку на панель инструментов)
   1. Сейчас при вырезании и последующей вставке нескольких связанных
блоков все связи между вставленными блоками удаляются. Нужно, чтобы
все связи оставались после вставки как они были до вырезания (должно
быть несложно, т.к. сейчас если блоки сгруппировать, а затем
вырезать-вставить, то связи остаются нетронутыми).
   1. Для каждого блока возможность двойным кликом на блоке поменять его
название (не тип), причём прямо на месте (т.е. текстовое поле должно
появляться поверх блока, при нажатии Enter - запоминается новое название
и текстовое поле исчезает, при нажатии Escape или клике в любую зону вне
блока, текстовое поле исчезает, введенное значение не запоминается). У
текстового поля не должно быть видимых границ. Если у блока уже есть
название, оно отображается в появившемся текстовом поле (т.е. поле с
названием в таком случае во время своего появления будет не пустым). У
всех блоков, которым присвоено название, оно всегда должно
отображаться по центру блока и быть привязанным к перемещениям блока.
   1. При перетаскивании блока мышкой, при близком поднесении одного блока к
другому (при условии, что они ещё не соединены) должна появляться
визуальная подсказка в виде полупрозрачной иконки между ними, что если
сейчас отпустить кнопку мыши, то блоки будут автоматически связаны
(связь от более старого блока к более новому по кратчайшей траектории -
то есть между ближайшими точками привязки одного и второго блока, для
которых такое соединение не противоречит конфигурациям блоков). Если
не отпускать кнопку мыши, а отвести блок дальше заданного расстояния от
другого блока, пиктограмма автосвязи исчезнет и при отпускании кнопки
мыши связь построена не будет. Расстояние, на котором таикм образом
должна возникать автосвязь между блоками - 50 пикселей и ближе.

1. **Добавить возможность привязки к каждому блоку набора свойств**
   1. Для каждого типа объекта предусмотреть набор свойств в виде пар “имя =
значение”. Имя и значение текстовые. Набор свойств каждого типа объектов
(т.е. набор “имён”) свой и хранится в его конфигурации (там же, где
хранится его графическое отображение). Для каждого свойства из таблицы
свойств помимо имени в конфигурации типа объекта хранится ещё тип
данных свойства (String, Int32, Bool и т.д.), а также значение по умолчанию
этого свойства (текстовое) и текст всплывающей подсказки для этого
свойства (тоже текст). При добавлении нового объекта из toolbox на
рабочее поле дизайнера у этого объекта появляется набор свойств
согласно определенному в конфигурации его типа, причем каждое свойство
имеет значение по умолчанию, как это определено в конфигурации.
   1. Добавить справа от поля дизайнера ещё одну панель с Expander и туда
вставить PropertyGrid. При выделении любого блока на диаграмме в
PropertyGrid появляются его свойства. При изменении любого свойства они
сохраняются в объекте блока.
Вот варианты компонентов для реализации PropertyGrid, но вы также
можете предложить свой: 
      * <https://github.com/DenysVuika/WPG>
      * <https://github.com/xceedsoftware/wpftoolkit/wiki> (PropertyGrid)
      * <https://github.com/xceedsoftware/wpftoolkit/wiki/PropertyGrid>
      * <https://www.codeproject.com/Articles/1092748/WPF-PropertyGrid-2>
      * <https://www.codeproject.com/Articles/87715/Native-WPF-PropertyGrid>  
Предложенный вами вариант необходимо согласовать.
   1. При сохранении диаграммы в файл должны сохраняться все свойства всех
блоков. При загрузке диаграммы из файла должны загружаться также
свойства всех объектов.

1. **Изменить способ загрузки библиотеки блоков и добавить несколько новых
настроек для типов блоков**
   1. Сейчас конфигурация типов блоков и состав палитры (toolbox) задаётся
внутри файла ресурсов приложения. Нужна возможность задавать
конфигурацию блоков во внешнем файле (файлах), а не во внутренних
ресурсах (или не только в них). При старте программы нужно сканировать
заданную папку (папку приложения) на предмет наличия новых файлов
типов, подгружать их и создавать для них новые toolbox при необходимости.
   1. Для каждого типа блоков возможность прописать в конфигурации типа,
сколько у этого блока точек привязки и с какой стороны (т.е. сделать
невидимыми и неактивными точки привязки с какой-то стороны или
добавить новые точки привязки). Могут существовать блоки, у которых все
точки привязки заблокированы (отсутствуют). Также нужна возможность
указать максимальное количество отдельно входящих и исходящих связей.
Т.е. например у блока может быть максимум одна входящая точка привязки
и максимум две исходящие точки привязки. Если попытаться привязать ещё
одну входящую связь, когда одна уже использована, программа не должна
давать это сделать (например, рисовать красный крестик вместо точки
привязки, можно предложить и другие варианты).
   1. Для каждого типа блоков возможность прописать в конфигурации типа, для
каждой его точки привязки - она допускает только исходящие связи, или
только входящие, или оба варианта.
   1. Для каждого типа блоков возможность прописать в конфигурации типа
стандартную подпись, которая будет появляться рядом с его точками
привязки (возможно, снаружи блока). Для каждой из точек привязки блока
эта надпись может быть своя.
   1. Для каждого Toolbox в его конфигурации может быть задан один из двух
способов отображения его Toolbox Items в программе. Один способ - как
сейчас, только под каждым типом блока сразу показывается его подпись (то
что сейчас во всплывающих подсказках). Всплывающие подсказки
остаются, но в них будут более длинные тексты. Второй способ
отображения - в виде списка, в котором маленькая иконка блока показана
слева, а текстовое название блока - справа. В остальном поведение этих
способов отображения одинаково.
   1. Если соответствующее свойство (nodelete = true) указано в конфигурации
типа блока, то его нельзя удалить.
   1. Если соответствующее свойство (invisible = true) указано в конфигурации
типа блока, то он не отображается в toolbox (хотя в диаграммах
существовать может)
   1. Если соответствующее свойство (proportional = true) указано в конфигурации
типа блока, то для блоков этого типа запретить непропорциональное
изменение их размеров. То есть при попытке изменить размеры блока
мышкой блока высота и ширина должны меняться одновременно и
пропорционально.
   1. Если соответствующее свойство (container = true) указано в конфигурации
типа блока, то на нём помимо названия в заданном месте располагается
надпись “Dbl. click to open”. У такого блока существует специальное
свойство - связанный файл. Этот файл открывается в отдельной вкладке
как отдельная диаграмма, если он уже был задан в свойстве такого блока.
Если же такой файл ещё не был связан, то при первом двойном клике по
такому блоку этот файл автоматически создаётся в той же папке, что и
текущий файл диаграммы, его название соответствует названию данного
блока. После создания файла диаграммы она открывается в новой вкладке
и эта вкладка отображается на экране.

1. **Исправить и дополнить несколько общих функций приложения**
   1. Добавить возможность вертикальной и горизонтальной прокрутки (pan)
области отображения при зажатой средней кнопке мыши.
   1. Сделать приложение мультиязычным. Автоматически определять локаль,
если русская - задавать русские константы, если любой другой язык -
английские. Предусмотреть окошко настроек с настройкой языка, чтобы его
можно было сменить и сохранить в настройках. Для локализации
использовать стандартный механизм Visual Studio.
   1. Кнопки Forward и Backward на панели инструментов не работают -
проверить и исправить.
   1. Кнопки Horizontal и Vertical на панели инструментов - работают странно или
не работают - проверить и исправить
   1. При создании новой диаграммы она должна создаваться не с нуля, а как
копия заданного константного шаблона (файла).
   1. Предусмотреть возможность открытия нескольких файлов, работы с
несколькими диаграммами одновременно. Открытые файлы появляются в
виде закладок зоны Diagram. В правом углу закладки каждого открытого
документа располагается крестик, который позволяет закрыть документ.
Название закладки отображает название открытого файла без его
расширения.
   1. Изменить стиль окна и Toolbar так, чтобы он был больше похож на Windows
10, а не на Windows 8, как сейчас (т.е. убрать лишние границы и градиенты,
немного поменять цвета).