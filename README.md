﻿# Параллельный упаковщик (архиватор) файла

ParallelPacker compress/decompress [source file path] [result file path]

ParallelPacker pack/unpack [source file path] [result file path]

**Платформа**: .NET Core 3.0

## О задаче:
Задача похожа на более сложную вариацию типовой [Producer/Consumer проблемы](https://en.wikipedia.org/wiki/Producer%E2%80%93consumer_problem). 
Если полагать, что поблочное чтение исходного и поблочная запись результирующего файла не нуждается в распараллеливании (не имеет смысла или не возможна) и параллельно будет происходить только преобразование (упаковка/распаковка), то можно адаптировать Producer/Consumer-паттерн для решения задачи, см. простой пример мой вариант [Java Locking Queue](https://github.com/pp-chain/jalgo/blob/master/src/com/jalgo/concurrency/LockingQueue.java).

## Общее описание:
Процесс архивации (упаковки) можно представить себе как сложный конвейер, в котором субконвейеры работают как источники и/или приёмщики данных от работников. Субконвейер может только отдавать или только принимать данные, либо и отдавать и принимать. Работники берут данные из одного субконвейера, преобразуют (если нужно) и кладут на другой.
В начале происходит взятие [i]-ой части исходного файла (блока с индексом [i]) одним работником и передача его дальше в центр. В центре множество работников параллельно производят преобразование (упаковку/распаковку) полученных блоков и передают результат в конец. В конце происходит сохранение в результирующий файл также одним работником (так как блоки могут прийти не по порядку индекса [i], то перед сохранением в результирующий файл, нужно либо упорядочивать их, либо сохранять с индексом).

## [Схема логики конвейера](https://drive.google.com/file/d/1Xa7sadd9VgMqtw_lsrOT5gLG-f-csY6S/view):
![Schema](https://github.com/pp-chain/ParallelPacker/blob/master/ParallelPackerSchema.png "Общая схема")
**Начало-Взятие** (взятие данных из исходного файла, формирование блоков и отдача в субконвейер считанных блоков).

**Центр-Параллельное-Преобразование** (взятие блоков из субконвейера считанных блоков, преобразование и отдача в субконвейер результирующих блоков).

**Конец-Сохранение** (взятие блоков из субконвейера результирующих блоков, а затем отдача в результирующий файл).

## Для синхронизации потоков-воркеров, работающих с субконвейерами на взятие/отдачу, можно использовать множество разных вариантов:
1) «Паттерн условной переменной» из «Рихтер Джеффри, CLR via C#. Программирование на платформе Microsoft.NET Framework 4.5 на языке C#, стр. 800», который использует a priori более быструю по сравнению с объектами ядра конструкцию Monitor. Но крайне важно понимать, что класс Monitor реализует привязку потока - поток, который получил блокировку, должен сам снять ее, вызвав метод Monitor.Exit, что, теоретически, может быть неоптимально в определённых сценариях, не смотря на координацию взаимодействия потоков посредством Monitor.Wait, Monitor.Pulse и Monitor.PulseAll.
2) Semaphor’ы (и их Slim-варианты для синхронизации внутри одного процесса), так как семафор не реализует привязку потока, поток может занять семафор, а другой поток может его освободить.
3) Идеальный вариант - не использовать блокировки/локи, а использовать Interlocked - это повод для будущего исследования.

## Ограничения текущей версии 
1) Не поддерживается стандартный формат GZip, то есть стандартный GZip-архиватор не распознает формат результирующего файла ParallelPacker. Необходимо дорабатывать, изучив [GZip формат](http://www.zlib.org/rfc-gzip.html).
2) При сохранении упакованного файла блоки не упорядочены по индексу. Можно реализовать Min Heap (очередь с приоритетом) по индексу блока и не записывать, пока не придёт следующий.

## Комментарии
Библиотеку можно обобщить не только на случай упаковки другим архиватором (для этого достаточно реализовать [IPackable](https://github.com/pp-chain/ParallelPacker/blob/master/ParallelPacker/PackerEngines/IPackable.cs)), но и на любой случай распараллеливания, подходящий под схему процесса ParallelPacker