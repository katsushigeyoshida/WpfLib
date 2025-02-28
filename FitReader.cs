using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace WpfLib
{
    //  FIT File データの構成
    //  参照: FIT SDK https://developer.garmin.com/fit/protocol/
    //      [File Header]  12byte
    //      [Data Records] x n データ
    //      [CRC]          2byte
    //  Data Record (3種類)
    //      1. [Normal Header][Definition Message]      格納するデータの定義
    //      2. [Normal Header][Data Message]            格納したデータ値
    //      3. [Time Offset(Compressed Timestamp) Header][Data Message]
    //  File Header
    //      [Header size][Protocol Version][Profile Version][Data Size][Data Type][CRC]
    //  Normal Header
    //      [Normal Header][Message Type][Message Type Specific][Local Message Type]
    //  Compressed Timestamp Header
    //      [Compressed Timestamp Header][Local Message Type][Time Offset(seconds)]
    //  Definition Message
    //      [Reserved][Architecture][Global Message Number][Fields][Field Definition x Fields]([Developer Fields][Developer Field Definition])
    //  Data Message (Definition Messageで定義されたデータの値が同じ順序で格納されている)
    //      [Field Value] x Fields
    //  Field Definition
    //      [Field Definition Number][Size][Base Type]
    //  Definition Message
    //      [Endian Ability][Reserved][BaseType Number]
    //  Developer Field
    //      [Field Number][Size][Developer Data Index]

    //  FitReaderの使い方
    //
    //  FitReader fitReader = new FitReader(fitFile);
    //  Console.WriteLine(fitReader.FileHeaderString());
    //  if (false) {
    //      //  CSV形式全出力
    //      int n = 0;
    //      bool csv = true;
    //      string buf;
    //      do {
    //          buf = fitReader.getDataRecordSting(csv);
    //          Console.WriteLine(buf);
    //      } while (0 < buf.Length && 0 < fitReader.mBackSize) ;
    //  } else {
    //      //  GPSデータの出力　
    //      int count = fitReader.getDataRecordAll(FitReader.DATATYPE.gpxData);
    //      Console.WriteLine($"GPS Data Count = {count}");
    //      List<GpsData> gpsDatas = fitReader.mListGpsData;
    //      foreach (var gpsData in gpsDatas) {
    //          Console.WriteLine(gpsData.toString());
    //      }
    //      Console.WriteLine(fitReader.getGpsDataInfo());
    //  }

    /// <summary>
    /// Field データ
    /// </summary>
    public class Field
    {
        public string name;     //  データ名
        public int num;         //  データのID
        public int type;        //  データの種別(byte,Int16,Int32....)
        public double scale;    //  データ値のスケール
        public int offset;      //  オフセット
        public string units;    //  データの単位表記

        public Field(string name, int num, int type, double scale, int offset, string units)
        {
            this.name = name;
            this.num = num;
            this.type = type;
            this.scale = scale;
            this.offset = offset;
            this.units = units;
        }
    }

    /// <summary>
    /// FileHeader Header: ファイルの種別を示すHeader 
    /// Table 1. Byte Description of File Header (FIT SDK)
    /// </summary>
    class FileHeader
    {
        public int HeaderSize;      //  このヘッダのサイズ(最小12byte))
        public int ProtcolVersion;  //  SDKのプロトコルバージョン
        public int ProfileVersion;  //  SDKのプロファイルバージョン
        public int DataSize;        //  データのサイズ
        public string DataType;     //  データの種別(".FIT"と記載)
        public int CRC = 0;         //  CRC

        public int size;            //  このデータのサイズ(データには含まれない)

        public FileHeader(byte[] data)
        {
            HeaderSize = (int)data[0];
            ProtcolVersion = (int)data[1];
            ProfileVersion = BitConverter.ToInt16(data, 2);
            DataSize = BitConverter.ToInt32(data, 4);
            DataType = YLib.binary2AsciiString(data, 8, 4);
            if (14 <= HeaderSize)
                CRC = BitConverter.ToInt16(data, 12);
            size = HeaderSize;
        }

        public string toString()
        {
            return $"HeaderSize:{HeaderSize} ProtcolVer:{ProtcolVersion} ProfileVer:{ProfileVersion} DataSize:{DataSize} DataType:{DataType} CRC:{CRC}";
        }
    }

    /// <summary>
    /// Record Header: Definition MessageとData MessageのHeader
    /// Table 2.Normal Header/Table 3. Compressed Timestamp Header (FIT SDK)
    /// </summary>
    class RecordHeader
    {
        private string[] headerType = new string[] {
                "Normal", "CompressedTimestamp"
            };
        private string[] MessageTypeStr = new string[] {
                    "Data Message", "Definition Message"
            };

        public bool Normal;             //　Headerの種別(Normal/CompressedTimestamp
        public int MessageType;         //  Messageの種別(0:DataMessage/1:DefinitionMessage
        public bool MessageTypeSpecific;//  特殊Message(DevelopperDataFlag)
        public int LocalMessageType;    //  
        public int TimeOffset;          //  seconds

        public int size;                //  このデータのサイズ(データには含まれない)

        public RecordHeader(byte data)
        {
            Normal = (data & 0x80) == 0;    //  Normal/CompressedTimestamp
            if (Normal) {
                //  Normal Header
                MessageType = (data & 0x40) >> 6;
                MessageTypeSpecific = (data & 0x20) != 0;
                LocalMessageType = (data & 0x0F);
            } else {
                //  Compress Timestamp Header
                LocalMessageType = (data >> 5 & 0x3);
                TimeOffset = data & 0x1f;
            }
            size = 1;
        }

        public string toString()
        {
            string buf = $"RecordlHeader:[{headerType[Normal ? 0 : 1]}]";
            if (Normal)
                return buf + $" MessageType:[{MessageTypeStr[MessageType]}] MessageTypeSecific:[{MessageTypeSpecific}] LocaMessageType:[{LocalMessageType}]";
            else
                return buf + $" LocalMessageType:[{LocalMessageType}] TimeOffset:[{TimeOffset}]";
        }
    }

    /// <summary>
    /// データの属性を表すデータ
    /// </summary>
    class GlobalMessage
    {
        //  GlobalMessageNumber FitSDK Fit\Profilr\Types\MsgNum.cs
        public static Dictionary<int, string> mFieldName = new Dictionary<int, string>() {
            {   0, "FileId" },
            {   1, "Capabilities"},
            {   2, "DeviceSettings"},
            {   3, "UserProfile"},
            {   4, "HrmProfile"},
            {   5, "SdmProfile"},
            {   6, "BikeProfile"},
            {   7, "ZonesTarget"},
            {   8, "HrZone"},
            {   9, "PowerZone"},
            {  10, "MetZone"},
            {  12, "Sport"},
            {  15, "Goal"},
            {  18, "Session"},
            {  19, "Lap"},
            {  20, "Record"},
            {  21, "Event"},
            {  23, "DeviceInfo"},
            {  26, "Workout"},
            {  27, "WorkoutStep"},
            {  28, "Schedule"},
            {  30, "WeightScale"},
            {  31, "Course"},
            {  32, "CoursePoint"},
            {  33, "Totals"},
            {  34, "Activity"},
            {  35, "Software"},
            {  37, "FileCapabilities"},
            {  38, "MesgCapabilities"},
            {  39, "FieldCapabilities"},
            {  49, "FileCreator"},
            {  51, "BloodPressure"},
            {  53, "SpeedZone"},
            {  55, "Monitoring"},
            {  72, "TrainingFile"},
            {  78, "Hrv"},
            {  80, "AntRx"},
            {  81, "AntTx"},
            {  82, "AntChannelId"},
            { 101, "Length"},
            { 103, "MonitoringInfo"},
            { 105, "Pad"},
            { 106, "SlaveDevice"},
            { 127, "Connectivity"},
            { 128, "WeatherConditions"},
            { 129, "WeatherAlert"},
            { 131, "CadenceZone"},
            { 132, "Hr"},
            { 142, "SegmentLap"},
            { 145, "MemoGlob"},
            { 148, "SegmentId"},
            { 149, "SegmentLeaderboardEntry"},
            { 150, "SegmentPoint"},
            { 151, "SegmentFile"},
            { 158, "WorkoutSession"},
            { 159, "WatchfaceSettings"},
            { 160, "GpsMetadata"},
            { 161, "CameraEvent"},
            { 162, "TimestampCorrelation"},
            { 164, "GyroscopeData"},
            { 165, "AccelerometerData"},
            { 167, "ThreeDSensorCalibration"},
            { 169, "VideoFrame"},
            { 174, "ObdiiData"},
            { 177, "NmeaSentence"},
            { 178, "AviationAttitude"},
            { 184, "Video"},
            { 185, "VideoTitle"},
            { 186, "VideoDescription"},
            { 187, "VideoClip"},
            { 188, "OhrSettings"},
            { 200, "ExdScreenConfiguration"},
            { 201, "ExdDataFieldConfiguration"},
            { 202, "ExdDataConceptConfiguration"},
            { 206, "FieldDescription"},
            { 207, "DeveloperDataId"},
            { 208, "MagnetometerData"},
            { 209, "BarometerData"},
            { 210, "OneDSensorCalibration"},
            { 216, "TimeInZone"},
            { 225, "Set"},
            { 227, "StressLevel"},
            { 258, "DiveSettings"},
            { 259, "DiveGas"},
            { 262, "DiveAlarm"},
            { 264, "ExerciseTitle"},
            { 268, "DiveSummary"},
            { 285, "Jump"},
            { 312, "Split"},
            { 317, "ClimbPro"},
            { 319, "TankUpdate"},
            { 323, "TankSummary"},
            { 375, "DeviceAuxBatteryInfo"},
            { 393, "DiveApneaAlarm"},
            { 0xFF00, "MfgRangeMin"}, // 0xFF00 - 0xFFFE reserved for manufacturer specific messages
            { 0xFFFE, "MfgRangeMax"}, // 0xFF00 - 0xFFFE reserved for manufacturer specific messages
            { 0xFFFF, "Invalid"},
        };

        //  FitSDK Fit\Profile.cs
        //  GlobalMessageNumber = 0(FileId) CreateFileIdMesg()
        public static List<Field> mFieldList = new List<Field>() {
            //        name, num, type, scale, offset, units
            new Field("Type", 0, 0, 1, 0, ""),
            new Field("Manufacturer", 1, 132, 1, 0, ""),
            new Field("Product", 2, 132, 1, 0, ""),
            new Field("SerialNumber", 3, 140, 1, 0, ""),
            new Field("TimeCreated", 4, 134, 1, 0, ""),
            new Field("Number", 5, 132, 1, 0, ""),
            new Field("ProductName", 8, 7, 1, 0, ""),
        };

        //  FitSDK Fit\Profile.cs
        //  GlobalMessageNumber = 18(Session) CreateLapMesg()
        public static List<Field> mSessionList = new List<Field>() {
            //        name, num, type, scale, offset, units
            new Field("MessageIndex", 254, 132, 1, 0, ""),
            new Field("Timestamp", 253, 134, 1, 0, "s"),
            new Field("Event", 0, 0, 1, 0, ""),
            new Field("EventType", 1, 0, 1, 0, ""),
            new Field("StartTime", 2, 134, 1, 0, ""),
            new Field("StartPositionLat", 3, 133, 1, 0, "semicircles"),
            new Field("StartPositionLong", 4, 133, 1, 0, "semicircles"),
            new Field("Sport", 5, 0, 1, 0, ""),
            new Field("SubSport", 6, 0, 1, 0, ""),
            new Field("TotalElapsedTime", 7, 134, 1000, 0, "s"),
            new Field("TotalTimerTime", 8, 134, 1000, 0, "s"),
            new Field("TotalDistance", 9, 134, 100, 0, "m"),
            new Field("TotalCycles", 10, 134, 1, 0, "cycles"),
            new Field("TotalCalories", 11, 132, 1, 0, "kcal"),
            new Field("TotalFatCalories", 13, 132, 1, 0, "kcal"),
            new Field("AvgSpeed", 14, 132, 1000, 0, "m/s"),
            new Field("MaxSpeed", 15, 132, 1000, 0, "m/s"),
            new Field("AvgHeartRate", 16, 2, 1, 0, "bpm"),
            new Field("MaxHeartRate", 17, 2, 1, 0, "bpm"),
            new Field("AvgCadence", 18, 2, 1, 0, "rpm"),
            new Field("MaxCadence", 19, 2, 1, 0, "rpm"),
            new Field("AvgPower", 20, 132, 1, 0, "watts"),
            new Field("MaxPower", 21, 132, 1, 0, "watts"),
            new Field("TotalAscent", 22, 132, 1, 0, "m"),
            new Field("TotalDescent", 23, 132, 1, 0, "m"),
            new Field("TotalTrainingEffect", 24, 2, 10, 0, ""),
            new Field("FirstLapIndex", 25, 132, 1, 0, ""),
            new Field("NumLaps", 26, 132, 1, 0, ""),
            new Field("EventGroup", 27, 2, 1, 0, ""),
            new Field("Trigger", 28, 0, 1, 0, ""),
            new Field("NecLat", 29, 133, 1, 0, "semicircles"),
            new Field("NecLong", 30, 133, 1, 0, "semicircles"),
            new Field("SwcLat", 31, 133, 1, 0, "semicircles"),
            new Field("SwcLong", 32, 133, 1, 0, "semicircles"),
            new Field("NumLengths", 33, 132, 1, 0, "lengths"),
            new Field("NormalizedPower", 34, 132, 1, 0, "watts"),
            new Field("TrainingStressScore", 35, 132, 10, 0, "tss"),
            new Field("IntensityFactor", 36, 132, 1000, 0, "if"),
            new Field("LeftRightBalance", 37, 132, 1, 0, ""),
            new Field("AvgStrokeCount", 41, 134, 10, 0, "strokes/lap"),
            new Field("AvgStrokeDistance", 42, 132, 100, 0, "m"),
            new Field("SwimStroke", 43, 0, 1, 0, "swim_stroke"),
            new Field("PoolLength", 44, 132, 100, 0, "m"),
            new Field("ThresholdPower", 45, 132, 1, 0, "watts"),
            new Field("PoolLengthUnit", 46, 0, 1, 0, ""),
            new Field("NumActiveLengths", 47, 132, 1, 0, "lengths"),
            new Field("TotalWork", 48, 134, 1, 0, "J"),
            new Field("AvgAltitude", 49, 132, 5, 500, "m"),
            new Field("MaxAltitude", 50, 132, 5, 500, "m"),
            new Field("GpsAccuracy", 51, 2, 1, 0, "m"),
            new Field("AvgGrade", 52, 131, 100, 0, "%"),
            new Field("AvgPosGrade", 53, 131, 100, 0, "%"),
            new Field("AvgNegGrade", 54, 131, 100, 0, "%"),
            new Field("MaxPosGrade", 55, 131, 100, 0, "%"),
            new Field("MaxNegGrade", 56, 131, 100, 0, "%"),
            new Field("AvgTemperature", 57, 1, 1, 0, "C"),
            new Field("MaxTemperature", 58, 1, 1, 0, "C"),
            new Field("TotalMovingTime", 59, 134, 1000, 0, "s"),
            new Field("AvgPosVerticalSpeed", 60, 131, 1000, 0, "m/s"),
            new Field("AvgNegVerticalSpeed", 61, 131, 1000, 0, "m/s"),
            new Field("MaxPosVerticalSpeed", 62, 131, 1000, 0, "m/s"),
            new Field("MaxNegVerticalSpeed", 63, 131, 1000, 0, "m/s"),
            new Field("MinHeartRate", 64, 2, 1, 0, "bpm"),
            new Field("TimeInHrZone", 65, 134, 1000, 0, "s"),
            new Field("TimeInSpeedZone", 66, 134, 1000, 0, "s"),
            new Field("TimeInCadenceZone", 67, 134, 1000, 0, "s"),
            new Field("TimeInPowerZone", 68, 134, 1000, 0, "s"),
            new Field("AvgLapTime", 69, 134, 1000, 0, "s"),
            new Field("BestLapIndex", 70, 132, 1, 0, ""),
            new Field("MinAltitude", 71, 132, 5, 500, "m"),
            new Field("PlayerScore", 82, 132, 1, 0, ""),
            new Field("OpponentScore", 83, 132, 1, 0, ""),
            new Field("OpponentName", 84, 7, 1, 0, ""),
            new Field("StrokeCount", 85, 132, 1, 0, "counts"),
            new Field("ZoneCount", 86, 132, 1, 0, "counts"),
            new Field("MaxBallSpeed", 87, 132, 100, 0, "m/s"),
            new Field("AvgBallSpeed", 88, 132, 100, 0, "m/s"),
            new Field("AvgVerticalOscillation", 89, 132, 10, 0, "mm"),
            new Field("AvgStanceTimePercent", 90, 132, 100, 0, "percent"),
            new Field("AvgStanceTime", 91, 132, 10, 0, "ms"),
            new Field("AvgFractionalCadence", 92, 2, 128, 0, "rpm"),
            new Field("MaxFractionalCadence", 93, 2, 128, 0, "rpm"),
            new Field("TotalFractionalCycles", 94, 2, 128, 0, "cycles"),
            new Field("AvgTotalHemoglobinConc", 95, 132, 100, 0, "g/dL"),
            new Field("MinTotalHemoglobinConc", 96, 132, 100, 0, "g/dL"),
            new Field("MaxTotalHemoglobinConc", 97, 132, 100, 0, "g/dL"),
            new Field("AvgSaturatedHemoglobinPercent", 98, 132, 10, 0, "%"),
            new Field("MinSaturatedHemoglobinPercent", 99, 132, 10, 0, "%"),
            new Field("MaxSaturatedHemoglobinPercent", 100, 132, 10, 0, "%"),
            new Field("AvgLeftTorqueEffectiveness", 101, 2, 2, 0, "percent"),
            new Field("AvgRightTorqueEffectiveness", 102, 2, 2, 0, "percent"),
            new Field("AvgLeftPedalSmoothness", 103, 2, 2, 0, "percent"),
            new Field("AvgRightPedalSmoothness", 104, 2, 2, 0, "percent"),
            new Field("AvgCombinedPedalSmoothness", 105, 2, 2, 0, "percent"),
            new Field("SportIndex", 111, 2, 1, 0, ""),
            new Field("TimeStanding", 112, 134, 1000, 0, "s"),
            new Field("StandCount", 113, 132, 1, 0, ""),
            new Field("AvgLeftPco", 114, 1, 1, 0, "mm"),
            new Field("AvgRightPco", 115, 1, 1, 0, "mm"),
            new Field("AvgLeftPowerPhase", 116, 2, 0.7111111, 0, "degrees"),
            new Field("AvgLeftPowerPhasePeak", 117, 2, 0.7111111, 0, "degrees"),
            new Field("AvgRightPowerPhase", 118, 2, 0.7111111, 0, "degrees"),
            new Field("AvgRightPowerPhasePeak", 119, 2, 0.7111111, 0, "degrees"),
            new Field("AvgPowerPosition", 120, 132, 1, 0, "watts"),
            new Field("MaxPowerPosition", 121, 132, 1, 0, "watts"),
            new Field("AvgCadencePosition", 122, 2, 1, 0, "rpm"),
            new Field("MaxCadencePosition", 123, 2, 1, 0, "rpm"),
            new Field("EnhancedAvgSpeed", 124, 134, 1000, 0, "m/s"),
            new Field("EnhancedMaxSpeed", 125, 134, 1000, 0, "m/s"),
            new Field("EnhancedAvgAltitude", 126, 134, 5, 500, "m"),
            new Field("EnhancedMinAltitude", 127, 134, 5, 500, "m"),
            new Field("EnhancedMaxAltitude", 128, 134, 5, 500, "m"),
            new Field("AvgLevMotorPower", 129, 132, 1, 0, "watts"),
            new Field("MaxLevMotorPower", 130, 132, 1, 0, "watts"),
            new Field("LevBatteryConsumption", 131, 2, 2, 0, "percent"),
            new Field("AvgVerticalRatio", 132, 132, 100, 0, "percent"),
            new Field("AvgStanceTimeBalance", 133, 132, 100, 0, "percent"),
            new Field("AvgStepLength", 134, 132, 10, 0, "mm"),
            new Field("TotalAnaerobicTrainingEffect", 137, 2, 10, 0, ""),
            new Field("AvgVam", 139, 132, 1000, 0, "m/s"),
            new Field("AvgDepth", 140, 134, 1000, 0, "m"),
            new Field("MaxDepth", 141, 134, 1000, 0, "m"),
            new Field("SurfaceInterval", 142, 134, 1, 0, "s"),
            new Field("StartCns", 143, 2, 1, 0, "percent"),
            new Field("EndCns", 144, 2, 1, 0, "percent"),
            new Field("StartN2", 145, 132, 1, 0, "percent"),
            new Field("EndN2", 146, 132, 1, 0, "percent"),
            new Field("AvgRespirationRate", 147, 2, 1, 0, ""),
            new Field("MaxRespirationRate", 148, 2, 1, 0, ""),
            new Field("MinRespirationRate", 149, 2, 1, 0, ""),
            new Field("MinTemperature", 150, 1, 1, 0, "C"),
            new Field("O2Toxicity", 155, 132, 1, 0, "OTUs"),
            new Field("DiveNumber", 156, 134, 1, 0, ""),
            new Field("TrainingLoadPeak", 168, 133, 65536, 0, ""),
            new Field("EnhancedAvgRespirationRate", 169, 132, 100, 0, "Breaths/min"),
            new Field("EnhancedMaxRespirationRate", 170, 132, 100, 0, "Breaths/min"),
            new Field("EnhancedMinRespirationRate", 180, 132, 100, 0, ""),
            new Field("TotalGrit", 181, 136, 1, 0, "kGrit"),
            new Field("TotalFlow", 182, 136, 1, 0, "Flow"),
            new Field("JumpCount", 183, 132, 1, 0, ""),
            new Field("AvgGrit", 186, 136, 1, 0, "kGrit"),
            new Field("AvgFlow", 187, 136, 1, 0, "Flow"),
            new Field("TotalFractionalAscent", 199, 2, 100, 0, "m"),
            new Field("TotalFractionalDescent", 200, 2, 100, 0, "m"),
            new Field("AvgCoreTemperature", 208, 132, 100, 0, "C"),
            new Field("MinCoreTemperature", 209, 132, 100, 0, "C"),
            new Field("MaxCoreTemperature", 210, 132, 100, 0, "C"),
        };

        //  FitSDK Fit\Profile.cs
        //  GlobalMessageNumber = 19(Lap) CreateLapMesg()
        public static List<Field> mLapList = new List<Field>() {
            //        name, num, type, scale, offset, units
            new Field("MessageIndex", 254, 132, 1, 0, ""),
            new Field("Timestamp", 253, 134, 1, 0, "s"),
            new Field("Event", 0, 0, 1, 0, ""),
            new Field("EventType", 1, 0, 1, 0, ""),
            new Field("StartTime", 2, 134, 1, 0, ""),
            new Field("StartPositionLat", 3, 133, 1, 0, "semicircles"),
            new Field("StartPositionLong", 4, 133, 1, 0, "semicircles"),
            new Field("EndPositionLat", 5, 133, 1, 0, "semicircles"),
            new Field("EndPositionLong", 6, 133, 1, 0, "semicircles"),
            new Field("TotalElapsedTime", 7, 134, 1000, 0, "s"),
            new Field("TotalTimerTime", 8, 134, 1000, 0, "s"),
            new Field("TotalDistance", 9, 134, 100, 0, "m"),
            new Field("TotalCycles", 10, 134, 1, 0, "cycles"),
            new Field("TotalCalories", 11, 132, 1, 0, "kcal"),
            new Field("TotalFatCalories", 12, 132, 1, 0, "kcal"),
            new Field("AvgSpeed", 13, 132, 1000, 0, "m/s"),
            new Field("MaxSpeed", 14, 132, 1000, 0, "m/s"),
            new Field("AvgHeartRate", 15, 2, 1, 0, "bpm"),
            new Field("MaxHeartRate", 16, 2, 1, 0, "bpm"),
            new Field("AvgCadence", 17, 2, 1, 0, "rpm"),
            new Field("MaxCadence", 18, 2, 1, 0, "rpm"),
            new Field("AvgPower", 19, 132, 1, 0, "watts"),
            new Field("MaxPower", 20, 132, 1, 0, "watts"),
            new Field("TotalAscent", 21, 132, 1, 0, "m"),
            new Field("TotalDescent", 22, 132, 1, 0, "m"),
            new Field("Intensity", 23, 0, 1, 0, ""),
            new Field("LapTrigger", 24, 0, 1, 0, ""),
            new Field("Sport", 25, 0, 1, 0, ""),
            new Field("EventGroup", 26, 2, 1, 0, ""),
            new Field("NumLengths", 32, 132, 1, 0, "lengths"),
            new Field("NormalizedPower", 33, 132, 1, 0, "watts"),
            new Field("LeftRightBalance", 34, 132, 1, 0, ""),
            new Field("FirstLengthIndex", 35, 132, 1, 0, ""),
            new Field("AvgStrokeDistance", 37, 132, 100, 0, "m"),
            new Field("SwimStroke", 38, 0, 1, 0, ""),
            new Field("SubSport", 39, 0, 1, 0, ""),
            new Field("NumActiveLengths", 40, 132, 1, 0, "lengths"),
            new Field("TotalWork", 41, 134, 1, 0, "J"),
            new Field("AvgAltitude", 42, 132, 5, 500, "m"),
            new Field("MaxAltitude", 43, 132, 5, 500, "m"),
            new Field("GpsAccuracy", 44, 2, 1, 0, "m"),
            new Field("AvgGrade", 45, 131, 100, 0, "%"),
            new Field("AvgPosGrade", 46, 131, 100, 0, "%"),
            new Field("AvgNegGrade", 47, 131, 100, 0, "%"),
            new Field("MaxPosGrade", 48, 131, 100, 0, "%"),
            new Field("MaxNegGrade", 49, 131, 100, 0, "%"),
            new Field("AvgTemperature", 50, 1, 1, 0, "C"),
            new Field("MaxTemperature", 51, 1, 1, 0, "C"),
            new Field("TotalMovingTime", 52, 134, 1000, 0, "s"),
            new Field("AvgPosVerticalSpeed", 53, 131, 1000, 0, "m/s"),
            new Field("AvgNegVerticalSpeed", 54, 131, 1000, 0, "m/s"),
            new Field("MaxPosVerticalSpeed", 55, 131, 1000, 0, "m/s"),
            new Field("MaxNegVerticalSpeed", 56, 131, 1000, 0, "m/s"),
            new Field("TimeInHrZone", 57, 134, 1000, 0, "s"),
            new Field("TimeInSpeedZone", 58, 134, 1000, 0, "s"),
            new Field("TimeInCadenceZone", 59, 134, 1000, 0, "s"),
            new Field("TimeInPowerZone", 60, 134, 1000, 0, "s"),
            new Field("RepetitionNum", 61, 132, 1, 0, ""),
            new Field("MinAltitude", 62, 132, 5, 500, "m"),
            new Field("MinHeartRate", 63, 2, 1, 0, "bpm"),
            new Field("WktStepIndex", 71, 132, 1, 0, ""),
            new Field("OpponentScore", 74, 132, 1, 0, ""),
            new Field("StrokeCount", 75, 132, 1, 0, "counts"),
            new Field("ZoneCount", 76, 132, 1, 0, "counts"),
            new Field("AvgVerticalOscillation", 77, 132, 10, 0, "mm"),
            new Field("AvgStanceTimePercent", 78, 132, 100, 0, "percent"),
            new Field("AvgStanceTime", 79, 132, 10, 0, "ms"),
            new Field("AvgFractionalCadence", 80, 2, 128, 0, "rpm"),
            new Field("MaxFractionalCadence", 81, 2, 128, 0, "rpm"),
            new Field("TotalFractionalCycles", 82, 2, 128, 0, "cycles"),
            new Field("PlayerScore", 83, 132, 1, 0, ""),
            new Field("AvgTotalHemoglobinConc", 84, 132, 100, 0, "g/dL"),
            new Field("MinTotalHemoglobinConc", 85, 132, 100, 0, "g/dL"),
            new Field("MaxTotalHemoglobinConc", 86, 132, 100, 0, "g/dL"),
            new Field("AvgSaturatedHemoglobinPercent", 87, 132, 10, 0, "%"),
            new Field("MinSaturatedHemoglobinPercent", 88, 132, 10, 0, "%"),
            new Field("MaxSaturatedHemoglobinPercent", 89, 132, 10, 0, "%"),
            new Field("AvgLeftTorqueEffectiveness", 91, 2, 2, 0, "percent"),
            new Field("AvgRightTorqueEffectiveness", 92, 2, 2, 0, "percent"),
            new Field("AvgLeftPedalSmoothness", 93, 2, 2, 0, "percent"),
            new Field("AvgRightPedalSmoothness", 94, 2, 2, 0, "percent"),
            new Field("AvgCombinedPedalSmoothness", 95, 2, 2, 0, "percent"),
            new Field("TimeStanding", 98, 134, 1000, 0, "s"),
            new Field("StandCount", 99, 132, 1, 0, ""),
            new Field("AvgLeftPco", 100, 1, 1, 0, "mm"),
            new Field("AvgRightPco", 101, 1, 1, 0, "mm"),
            new Field("AvgLeftPowerPhase", 102, 2, 0.7111111, 0, "degrees"),
            new Field("AvgLeftPowerPhasePeak", 103, 2, 0.7111111, 0, "degrees"),
            new Field("AvgRightPowerPhase", 104, 2, 0.7111111, 0, "degrees"),
            new Field("AvgRightPowerPhasePeak", 105, 2, 0.7111111, 0, "degrees"),
            new Field("AvgPowerPosition", 106, 132, 1, 0, "watts"),
            new Field("MaxPowerPosition", 107, 132, 1, 0, "watts"),
            new Field("AvgCadencePosition", 108, 2, 1, 0, "rpm"),
            new Field("MaxCadencePosition", 109, 2, 1, 0, "rpm"),
            new Field("EnhancedAvgSpeed", 110, 134, 1000, 0, "m/s"),
            new Field("EnhancedMaxSpeed", 111, 134, 1000, 0, "m/s"),
            new Field("EnhancedAvgAltitude", 112, 134, 5, 500, "m"),
            new Field("EnhancedMinAltitude", 113, 134, 5, 500, "m"),
            new Field("EnhancedMaxAltitude", 114, 134, 5, 500, "m"),
            new Field("AvgLevMotorPower", 115, 132, 1, 0, "watts"),
            new Field("MaxLevMotorPower", 116, 132, 1, 0, "watts"),
            new Field("LevBatteryConsumption", 117, 2, 2, 0, "percent"),
            new Field("AvgVerticalRatio", 118, 132, 100, 0, "percent"),
            new Field("AvgStanceTimeBalance", 119, 132, 100, 0, "percent"),
            new Field("AvgStepLength", 120, 132, 10, 0, "mm"),
            new Field("AvgVam", 121, 132, 1000, 0, "m/s"),
            new Field("AvgDepth", 122, 134, 1000, 0, "m"),
            new Field("MaxDepth", 123, 134, 1000, 0, "m"),
            new Field("MinTemperature", 124, 1, 1, 0, "C"),
            new Field("EnhancedAvgRespirationRate", 136, 132, 100, 0, "Breaths/min"),
            new Field("EnhancedMaxRespirationRate", 137, 132, 100, 0, "Breaths/min"),
            new Field("AvgRespirationRate", 147, 2, 1, 0, ""),
            new Field("MaxRespirationRate", 148, 2, 1, 0, ""),
            new Field("TotalGrit", 149, 136, 1, 0, "kGrit"),
            new Field("TotalFlow", 150, 136, 1, 0, "Flow"),
            new Field("JumpCount", 151, 132, 1, 0, ""),
            new Field("AvgGrit", 153, 136, 1, 0, "kGrit"),
            new Field("AvgFlow", 154, 136, 1, 0, "Flow"),
            new Field("TotalFractionalAscent", 156, 2, 100, 0, "m"),
            new Field("TotalFractionalDescent", 157, 2, 100, 0, "m"),
            new Field("AvgCoreTemperature", 158, 132, 100, 0, "C"),
            new Field("MinCoreTemperature", 159, 132, 100, 0, "C"),
            new Field("MaxCoreTemperature", 160, 132, 100, 0, "C"),
        };

        //  FitSDK Fit\Profile.cs
        //  GlobalMessageNumber = 20(Record) CreateRecordMesg()
        public static List<Field> mRecordList = new List<Field>() {
            //        name, num, type, scale, offset, units
            new Field("Timestamp", 253, 134, 1, 0, "s"),            //  1989/12/31 00:00:00を起点とした秒数
            new Field("PositionLat", 0, 133, 1, 0, "semicircles"),  //  semicircle: 180°を4byte整数の最大値(2^31)で表す
            new Field("PositionLong", 1, 133, 1, 0, "semicircles"), //  degrees = semicircle * (180 / 2^31)  (WGS84クラス)
            new Field("Altitude", 2, 132, 5, 500, "m"),
            new Field("HeartRate", 3, 2, 1, 0, "bpm"),
            new Field("Cadence", 4, 2, 1, 0, "rpm"),
            new Field("Distance", 5, 134, 100, 0, "m"),
            new Field("Speed", 6, 132, 1000, 0, "m/s"),
            new Field("Power", 7, 132, 1, 0, "watts"),
            new Field("CompressedSpeedDistance", 8, 13, 1, 0, ""),
            new Field("Grade", 9, 131, 100, 0, "%"),
            new Field("Resistance", 10, 2, 1, 0, ""),
            new Field("TimeFromCourse", 11, 133, 1000, 0, "s"),
            new Field("Temperature", 13, 1, 1, 0, "C"),
            new Field("Speed1s", 17, 2, 16, 0, "m/s"),
            new Field("Cycles", 18, 2, 1, 0, "cycles"),
            new Field("TotalCycles", 19, 134, 1, 0, "cycles"),
            new Field("CompressedAccumulatedPower", 28, 132, 1, 0, "watts"),
            new Field("AccumulatedPower", 29, 134, 1, 0, "watts"),
            new Field("LeftRightBalance", 30, 2, 1, 0, ""),
            new Field("GpsAccuracy", 31, 2, 1, 0, "m"),
            new Field("VerticalSpeed", 32, 131, 1000, 0, "m/s"),
            new Field("Calories", 33, 132, 1, 0, "kcal"),
            new Field("VerticalOscillation", 39, 132, 10, 0, "mm"),
            new Field("StanceTimePercent", 40, 132, 100, 0, "percent"),
            new Field("StanceTime", 41, 132, 10, 0, "ms"),
            new Field("ActivityType", 42, 0, 1, 0, ""),
            new Field("LeftTorqueEffectiveness", 43, 2, 2, 0, "percent"),
            new Field("RightTorqueEffectiveness", 44, 2, 2, 0, "percent"),
            new Field("LeftPedalSmoothness", 45, 2, 2, 0, "percent"),
            new Field("RightPedalSmoothness", 46, 2, 2, 0, "percent"),
            new Field("CombinedPedalSmoothness", 47, 2, 2, 0, "percent"),
            new Field("Time128", 48, 2, 128, 0, "s"),
            new Field("StrokeType", 49, 0, 1, 0, ""),
            new Field("Zone", 50, 2, 1, 0, ""),
            new Field("BallSpeed", 51, 132, 100, 0, "m/s"),
            new Field("Cadence256", 52, 132, 256, 0, "rpm"),
            new Field("FractionalCadence", 53, 2, 128, 0, "rpm"),
            new Field("TotalHemoglobinConc", 54, 132, 100, 0, "g/dL"),
            new Field("TotalHemoglobinConcMin", 55, 132, 100, 0, "g/dL"),
            new Field("TotalHemoglobinConcMax", 56, 132, 100, 0, "g/dL"),
            new Field("SaturatedHemoglobinPercent", 57, 132, 10, 0, "%"),
            new Field("SaturatedHemoglobinPercentMin", 58, 132, 10, 0, "%"),
            new Field("SaturatedHemoglobinPercentMax", 59, 132, 10, 0, "%"),
            new Field("DeviceIndex", 62, 2, 1, 0, ""),
            new Field("LeftPco", 67, 1, 1, 0, "mm"),
            new Field("RightPco", 68, 1, 1, 0, "mm"),
            new Field("LeftPowerPhase", 69, 2, 0.7111111, 0, "degrees"),
            new Field("LeftPowerPhasePeak", 70, 2, 0.7111111, 0, "degrees"),
            new Field("RightPowerPhase", 71, 2, 0.7111111, 0, "degrees"),
            new Field("RightPowerPhasePeak", 72, 2, 0.7111111, 0, "degrees"),
            new Field("EnhancedSpeed", 73, 134, 1000, 0, "m/s"),
            new Field("EnhancedAltitude", 78, 134, 5, 500, "m"),
            new Field("BatterySoc", 81, 2, 2, 0, "percent"),
            new Field("MotorPower", 82, 132, 1, 0, "watts"),
            new Field("VerticalRatio", 83, 132, 100, 0, "percent"),
            new Field("StanceTimeBalance", 84, 132, 100, 0, "percent"),
            new Field("StepLength", 85, 132, 10, 0, "mm"),
            new Field("AbsolutePressure", 91, 134, 1, 0, "Pa"),
            new Field("Depth", 92, 134, 1000, 0, "m"),
            new Field("NextStopDepth", 93, 134, 1000, 0, "m"),
            new Field("NextStopTime", 94, 134, 1, 0, "s"),
            new Field("TimeToSurface", 95, 134, 1, 0, "s"),
            new Field("NdlTime", 96, 134, 1, 0, "s"),
            new Field("CnsLoad", 97, 2, 1, 0, "percent"),
            new Field("N2Load", 98, 132, 1, 0, "percent"),
            new Field("RespirationRate", 99, 2, 1, 0, "s"),
            new Field("EnhancedRespirationRate", 108, 132, 100, 0, "Breaths/min"),
            new Field("Grit", 114, 136, 1, 0, ""),
            new Field("Flow", 115, 136, 1, 0, ""),
            new Field("EbikeTravelRange", 117, 132, 1, 0, "km"),
            new Field("EbikeBatteryLevel", 118, 2, 1, 0, "percent"),
            new Field("EbikeAssistMode", 119, 2, 1, 0, "depends on sensor"),
            new Field("EbikeAssistLevelPercent", 120, 2, 1, 0, "percent"),
            new Field("AirTimeRemaining", 123, 134, 1, 0, "s"),
            new Field("PressureSac", 124, 132, 100, 0, "bar/min"),
            new Field("VolumeSac", 125, 132, 100, 0, "L/min"),
            new Field("Rmv", 126, 132, 100, 0, "L/min"),
            new Field("AscentRate", 127, 133, 1000, 0, "m/s"),
            new Field("Po2", 129, 2, 100, 0, "percent"),
            new Field("CoreTemperature", 139, 132, 100, 0, "C"),
        };

        //  FitSDK Fit\Profile.cs
        //  GlobalMessageNumber = 21(Event) CreateEventMesg()
        public static List<Field> mEventList = new List<Field>() {
            //        name, num, type, scale, offset, units
            new Field("Timestamp", 253, 134, 1, 0, "s"),
            new Field("Event", 0, 0, 1, 0, ""),
            new Field("EventType", 1, 0, 1, 0, ""),
            new Field("Data16", 2, 132, 1, 0, ""),
            new Field("Data", 3, 134, 1, 0, ""),
            new Field("EventGroup", 4, 2, 1, 0, ""),
            new Field("Score", 7, 132, 1, 0, ""),
            new Field("OpponentScore", 8, 132, 1, 0, ""),
            new Field("FrontGearNum", 9, 10, 1, 0, ""),
            new Field("FrontGear", 10, 10, 1, 0, ""),
            new Field("RearGearNum", 11, 10, 1, 0, ""),
            new Field("RearGear", 12, 10, 1, 0, ""),
            new Field("DeviceIndex", 13, 2, 1, 0, ""),
            new Field("RadarThreatLevelMax", 21, 0, 1, 0, ""),
            new Field("RadarThreatCount", 22, 2, 1, 0, ""),
            new Field("RadarThreatAvgApproachSpeed", 23, 2, 10, 0, "m/s"),
            new Field("RadarThreatMaxApproachSpeed", 24, 2, 10, 0, "m/s"),
        };
        //  FitSDK Fit\Profile.cs
        //  GlobalMessageNumber = 23(DeviceInfo) CreateDeviceInfoMesg()
        public static List<Field> mDeviceInfoList = new List<Field>() {
            //        name, num, type, scale, offset, units
            new Field("Timestamp", 253, 134, 1, 0, "s"),
            new Field("DeviceIndex", 0, 2, 1, 0, ""),
            new Field("DeviceType", 1, 2, 1, 0, ""),
            new Field("Manufacturer", 2, 132, 1, 0, ""),
            new Field("SerialNumber", 3, 140, 1, 0, ""),
            new Field("Product", 4, 132, 1, 0, ""),
            new Field("SoftwareVersion", 5, 132, 100, 0, ""),
            new Field("HardwareVersion", 6, 2, 1, 0, ""),
            new Field("CumOperatingTime", 7, 134, 1, 0, "s"),
            new Field("BatteryVoltage", 10, 132, 256, 0, "V"),
            new Field("BatteryStatus", 11, 2, 1, 0, ""),
            new Field("SensorPosition", 18, 0, 1, 0, ""),
            new Field("Descriptor", 19, 7, 1, 0, ""),
            new Field("AntTransmissionType", 20, 10, 1, 0, ""),
            new Field("AntDeviceNumber", 21, 139, 1, 0, ""),
            new Field("AntNetwork", 22, 0, 1, 0, ""),
            new Field("SourceType", 25, 0, 1, 0, ""),
            new Field("ProductName", 27, 7, 1, 0, ""),
            new Field("BatteryLevel", 32, 2, 1, 0, "%"),
        };

        //  FitSDK Fit\Profile.cs
        //  GlobalMessageNumber = 34(Activity) CreateActivityMesg()
        public static List<Field> mActivityList = new List<Field>() {
            //        name, num, type, scale, offset, units
            new Field("Timestamp", 253, 134, 1, 0, ""),
            new Field("TotalTimerTime", 0, 134, 1000, 0, "s"),
            new Field("NumSessions", 1, 132, 1, 0, ""),
            new Field("Type", 2, 0, 1, 0, ""),
            new Field("Event", 3, 0, 1, 0, ""),
            new Field("EventType", 4, 0, 1, 0, ""),
            new Field("LocalTimestamp", 5, 134, 1, 0, ""),
            new Field("EventGroup", 6, 2, 1, 0, ""),
        };

        //  FitSDK Fit\Profile.cs
        //  GlobalMessageNumber = 101(Length) CreateLengthMesg()
        public static List<Field> mLengthList = new List<Field>() {
            //        name, num, type, scale, offset, units
            new Field("MessageIndex", 254, 132, 1, 0, ""),
            new Field("Timestamp", 253, 134, 1, 0, ""),
            new Field("Event", 0, 0, 1, 0, ""),
            new Field("EventType", 1, 0, 1, 0, ""),
            new Field("StartTime", 2, 134, 1, 0, ""),
            new Field("TotalElapsedTime", 3, 134, 1000, 0, "s"),
            new Field("TotalTimerTime", 4, 134, 1000, 0, "s"),
            new Field("TotalStrokes", 5, 132, 1, 0, "strokes"),
            new Field("AvgSpeed", 6, 132, 1000, 0, "m/s"),
            new Field("SwimStroke", 7, 0, 1, 0, "swim_stroke"),
            new Field("AvgSwimmingCadence", 9, 2, 1, 0, "strokes/min"),
            new Field("EventGroup", 10, 2, 1, 0, ""),
            new Field("TotalCalories", 11, 132, 1, 0, "kcal"),
            new Field("LengthType", 12, 0, 1, 0, ""),
            new Field("PlayerScore", 18, 132, 1, 0, ""),
            new Field("OpponentScore", 19, 132, 1, 0, ""),
            new Field("StrokeCount", 20, 132, 1, 0, "counts"),
            new Field("ZoneCount", 21, 132, 1, 0, "counts"),
            new Field("EnhancedAvgRespirationRate", 22, 132, 100, 0, "Breaths/min"),
            new Field("EnhancedMaxRespirationRate", 23, 132, 100, 0, "Breaths/min"),
            new Field("AvgRespirationRate", 24, 2, 1, 0, ""),
            new Field("MaxRespirationRate", 25, 2, 1, 0, ""),
        };

        //  FitSDK Fit\Profile.cs
        //  GlobalMessageNumber = 132(Hr) CreateHrMesg() (心拍センサー Heart Rate)
        public static List<Field> mHrList = new List<Field>() {
            //        name, num, type, scale, offset, units
            new Field("Timestamp", 253, 134, 1, 0, ""),
            new Field("FractionalTimestamp", 0, 132, 32768, 0, "s"),
            new Field("Time256", 1, 2, 256, 0, "s"),
            new Field("FilteredBpm", 6, 2, 1, 0, "bpm"),
            new Field("EventTimestamp", 9, 134, 1024, 0, "s"),
            new Field("EventTimestamp12", 10, 13, 1, 0, ""),
        };

        //  FitSDK Fit\Profile.cs
        //  GlobalMessageNumber = 206(FieldDescription) CreateFieldDescriptionMesg()
        public static List<Field> mFieldDescriptionList = new List<Field>() {
            //        name, num, type, scale, offset, units
            new Field("DeveloperDataIndex", 0, 2, 1, 0, ""),
            new Field("FieldDefinitionNumber", 1, 2, 1, 0, ""),
            new Field("FitBaseTypeId", 2, 2, 1, 0, ""),
            new Field("FieldName", 3, 7, 1, 0, ""),
            new Field("Array", 4, 2, 1, 0, ""),
            new Field("Components", 5, 7, 1, 0, ""),
            new Field("Scale", 6, 2, 1, 0, ""),
            new Field("Offset", 7, 1, 1, 0, ""),
            new Field("Units", 8, 7, 1, 0, ""),
            new Field("Bits", 9, 7, 1, 0, ""),
            new Field("Accumulate", 10, 7, 1, 0, ""),
            new Field("FitBaseUnitId", 13, 132, 1, 0, ""),
            new Field("NativeMesgNum", 14, 132, 1, 0, ""),
            new Field("NativeFieldNum", 15, 2, 1, 0, ""),
        };

        //  FitSDK Fit\Profile.cs
        //  GlobalMessageNumber = 207(DeveloperDataId) CreateDeveloperDataIdMesg()
        public static List<Field> mDeveloperDataIdList = new List<Field>() {
            //        name, num, type, scale, offset, units
            new Field("Timestamp", 253, 134, 1, 0, ""),
            new Field("TotalTimerTime", 0, 134, 1000, 0, "s"),
            new Field("NumSessions", 1, 132, 1, 0, ""),
            new Field("Type", 2, 0, 1, 0, ""),
            new Field("Event", 3, 0, 1, 0, ""),
            new Field("EventType", 4, 0, 1, 0, ""),
            new Field("LocalTimestamp", 5, 134, 1, 0, ""),
            new Field("EventGroup", 6, 2, 1, 0, ""),
        };

        /// <summary>
        /// Global Message NumberとField Numberからデータの属性を取得
        /// </summary>
        /// <param name="globalMessageNumber">Global Message Number</param>
        /// <param name="fieldNumber">Field Number</param>
        /// <returns>Field(データの属性)</returns>
        public static Field getFieldName(int globalMessageNumber, int fieldNumber)
        {
            if (globalMessageNumber == 0) {
                //  FileId
                int n = mFieldList.FindIndex(p => p.num == fieldNumber);
                return 0 <= n ? mFieldList[n] : null;
            } else if (globalMessageNumber == 18) {
                //  Session
                int n = mSessionList.FindIndex(p => p.num == fieldNumber);
                return 0 <= n ? mSessionList[n] : null;
            } else if (globalMessageNumber == 19) {
                //  Lap
                int n = mLapList.FindIndex(p => p.num == fieldNumber);
                return 0 <= n ? mLapList[n] : null;
            } else if (globalMessageNumber == 20) {
                //  Record
                int n = mRecordList.FindIndex(p => p.num == fieldNumber);
                return 0 <= n ? mRecordList[n] : null;
            } else if (globalMessageNumber == 21) {
                //  Event
                int n = mEventList.FindIndex(p => p.num == fieldNumber);
                return 0 <= n ? mEventList[n] : null;
            } else if (globalMessageNumber == 23) {
                //  DeviceInfo
                int n = mDeviceInfoList.FindIndex(p => p.num == fieldNumber);
                return 0 <= n ? mDeviceInfoList[n] : null;
            } else if (globalMessageNumber == 34) {
                //  Activity
                int n = mActivityList.FindIndex(p => p.num == fieldNumber);
                return 0 <= n ? mActivityList[n] : null;
            } else if (globalMessageNumber == 101) {
                //  Length
                int n = mLengthList.FindIndex(p => p.num == fieldNumber);
                return 0 <= n ? mLengthList[n] : null;
            } else if (globalMessageNumber == 132) {
                //  Hr
                int n = mHrList.FindIndex(p => p.num == fieldNumber);
                return 0 <= n ? mHrList[n] : null;
            } else if (globalMessageNumber == 206) {
                //  FieldDescription
                int n = mFieldDescriptionList.FindIndex(p => p.num == fieldNumber);
                return 0 <= n ? mFieldDescriptionList[n] : null;
            } else if (globalMessageNumber == 207) {
                //  DeveloperDataId
                int n = mDeveloperDataIdList.FindIndex(p => p.num == fieldNumber);
                return 0 <= n ? mDeveloperDataIdList[n] : null;
            }
            return null;
        }
    }

    //  Table 5. Field Definition (FIT SDK)
    /// <summary>
    /// Field 定義データ
    /// </summary>
    public class FieldDefinition
    {
        public int FieldDefinitionNumber;   //  データ定義番号
        public int Size;                    //  データサイズ
        public int BaseType;                //  データ種別(Table 6. Definition Message / Table 7. FIT Base Types and Invalid Values)

        public byte[] Data;                 //  データ値(DataMessageで使用)
        private int GlobalMessageNumber = 0;//  データの分類

        public int size;        //  このデータのサイズ(Dataa値は含まない)

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="data">byteデータ</param>
        /// <param name="start">データ位置</param>
        /// <param name="globalMessageNumber">データの分類番号</param>
        public FieldDefinition(byte[] data, int start, int globalMessageNumber)
        {
            FieldDefinitionNumber = (int)data[start];
            Size = (int)data[start + 1];
            BaseType = (int)data[start + 2];
            size = 3;
            GlobalMessageNumber = globalMessageNumber;
        }

        /// <summary>
        /// データをintで取り出す
        /// </summary>
        /// <returns>intデータ</returns>
        public int getInt()
        {
            if (Data != null && 0 < Data.Length) {
                return getDataValueInt(Data, BaseType);
            } else {
                return 0;
            }
        }

        /// <summary>
        /// データをdoubleで取り出す
        /// </summary>
        /// <returns>doubleデータ</returns>
        public double getDouble()
        {
            if (Data != null && 0 < Data.Length) {
                Field field = GlobalMessage.getFieldName(GlobalMessageNumber, FieldDefinitionNumber);
                if (field != null) {
                    if (field.units.CompareTo("semicircles") == 0) {
                        //  座標データ(緯度・経度(semicircle→度))
                        return getDataValueInt(Data, BaseType) / Math.Pow(2, 31) * 180;
                    } else {
                        return (double)getDataValueInt(Data, BaseType) / (double)field.scale - field.offset;
                    }
                }
            }
            return 0.0;
        }

        /// <summary>
        /// 日付データの取り出し
        /// </summary>
        /// <returns>DateTimeデータ</returns>
        public DateTime getDateTime()
        {
            if (Data != null && 0 < Data.Length) {
                Field field = GlobalMessage.getFieldName(GlobalMessageNumber, FieldDefinitionNumber);
                if (field.name.CompareTo("Timestamp") == 0 || field.name.CompareTo("StartTime") == 0 ||
                    field.name.CompareTo("LocalTimestamp") == 0) {
                    //  日時データ
                    var dt = getDataValueUInt(Data, BaseType);
                    if (dt != 0xffffffff)
                        return fitDate2DateTime(dt).ToLocalTime();
                }
            }
            return new DateTime();
        }

        /// <summary>
        /// データを文字列で取得
        /// </summary>
        /// <returns></returns>
        public string toString()
        {
            Field field = GlobalMessage.getFieldName(GlobalMessageNumber, FieldDefinitionNumber);
            string buf = $"FieldDefinitionNumber:{FieldDefinitionNumber}";
            if (field != null)
                buf += $"({field.name})";
            buf += $" Size:{Size} BaseType:0x{BaseType.ToString("X2")}";
            if (Data != null && 0 < Data.Length) {
                buf += $" [{getDataString(Data, BaseType)}]";
                buf += $" [{getDataValueString(field)}]";
            }
            return buf;
        }

        /// <summary>
        /// データをCSV形式で出力
        /// </summary>
        /// <returns></returns>
        public string toCsvString()
        {
            string buf = "";
            Field field = GlobalMessage.getFieldName(GlobalMessageNumber, FieldDefinitionNumber);
            if (Data != null && 0 < Data.Length) {
                buf += $"{getDataValueString(field)}";
            } else {
                if (field != null)
                    buf += $"{field.name}({field.units})";
            }
            return buf;
        }

        /// <summary>
        /// Fieldの種別に合わせてデータ値を求めて文字列にする
        /// </summary>
        /// <param name="globalMessageNumber">Global Message Number</param>
        /// <param name="field">Fieldデータ</param>
        /// <returns>データ値</returns>
        public string getDataValueString(Field field)
        {
            if (field == null)
                return getDataString(Data, BaseType);
            //  Record データ
            if (field.name.CompareTo("Timestamp") == 0 || field.name.CompareTo("StartTime") == 0 ||
                field.name.CompareTo("LocalTimestamp") == 0) {
                //  日時データ
                var dt = getDataValueUInt(Data, BaseType);
                if (dt == 0xffffffff)
                    return "";
                return fitDate2DateTime(dt).ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss");
            } else if (field.units.CompareTo("semicircles") == 0) {
                //  座標データ(緯度・経度(semicircle→度))
                return (getDataValueInt(Data, BaseType) / Math.Pow(2, 31) * 180).ToString("F6");
            } else {
                return ((double)getDataValueInt(Data, BaseType) / (double)field.scale - field.offset).ToString("F4");
            }
        }

        /// <summary>
        /// byte データからint データに変換
        /// </summary>
        /// <param name="data">byteデータ</param>
        /// <param name="baseType">データの種別</param>
        /// <returns>変換データ</returns>
        public int getDataValueInt(byte[] data, int baseType)
        {
            int buf = 0;
            if (data != null && 0 < data.Length) {
                switch (baseType) {
                    case 0x00: buf = data[0]; break;
                    case 0x01: buf = data[0]; break;
                    case 0x02: buf = data[0]; break;
                    case 0x83: buf = BitConverter.ToInt16(data, 0); break;
                    case 0x84: buf = BitConverter.ToUInt16(data, 0); break;
                    case 0x85: buf = BitConverter.ToInt32(data, 0); break;
                    case 0x86: buf = (int)BitConverter.ToUInt32(data, 0); break;
                    case 0x07: buf = data[0]; break;
                    //case 0x88: buf = BitConverter.ToSingle(data, 0); break;
                    //case 0x89: buf = BitConverter.ToDouble(data, 0); break;
                    case 0x8A: buf = data[0]; break;
                    case 0x8B: buf = BitConverter.ToUInt16(data, 0); break;
                    //case 0x8C: buf = BitConverter.ToUInt32(data, 0); break;
                    case 0x0D: buf = data[0]; break;
                        //case 0x8E: buf = BitConverter.ToUInt64(data, 0) ; break;
                        //case 0x8F: buf = BitConverter.ToUInt64(data, 0) ; break;
                        //case 0x90: buf = BitConverter.ToUInt64(data, 0) ; break;
                        //default: buf = YLib.binary2HexString(data, 0, Size); break;
                }
            }
            return buf;
        }

        /// <summary>
        /// byte データをUIntに変換
        /// </summary>
        /// <param name="data">byteデータ</param>
        /// <param name="baseType">データの種別</param>
        /// <returns>変換データ</returns>
        public uint getDataValueUInt(byte[] data, int baseType)
        {
            uint buf = 0;
            if (data != null && 0 < data.Length) {
                switch (baseType) {
                    case 0x00: buf = data[0]; break;
                    case 0x01: buf = data[0]; break;
                    case 0x02: buf = data[0]; break;
                    //case 0x83: buf = BitConverter.ToInt16(data, 0); break;
                    case 0x84: buf = BitConverter.ToUInt16(data, 0); break;
                    //case 0x85: buf = BitConverter.ToInt32(data, 0); break;
                    case 0x86: buf = BitConverter.ToUInt32(data, 0); break;
                    case 0x07: buf = data[0]; break;
                    //case 0x88: buf = BitConverter.ToSingle(data, 0); break;
                    //case 0x89: buf = BitConverter.ToDouble(data, 0); break;
                    case 0x8A: buf = data[0]; break;
                    case 0x8B: buf = BitConverter.ToUInt16(data, 0); break;
                    case 0x8C: buf = BitConverter.ToUInt32(data, 0); break;
                    case 0x0D: buf = data[0]; break;
                        //case 0x8E: buf = BitConverter.ToUInt64(data, 0); break;
                        //case 0x8F: buf = BitConverter.ToUInt64(data, 0); break;
                        //case 0x90: buf = BitConverter.ToUInt64(data, 0); break;
                        //default: buf = YLib.binary2HexString(data, 0, Size); break;
                }
            }
            return buf;
        }

        /// <summary>
        /// byte データをdouble データに変換
        /// </summary>
        /// <param name="data">byteデータ</param>
        /// <param name="baseType">データの種別</param>
        /// <returns>変換データ</returns>
        public double getDataValueDouble(byte[] data, int baseType)
        {
            double buf = 0;
            if (data != null && 0 < data.Length) {
                switch (baseType) {
                    //case 0x00: buf = data[0]; break;
                    //case 0x01: buf = data[0]; break;
                    //case 0x02: buf = data[0]; break;
                    //case 0x83: buf = BitConverter.ToInt16(data, 0); break;
                    //case 0x84: buf = BitConverter.ToUInt16(data, 0); break;
                    //case 0x85: buf = BitConverter.ToInt32(data, 0); break;
                    //case 0x86: buf = BitConverter.ToUInt32(data, 0); break;
                    //case 0x07: buf = data[0]; break;
                    case 0x88: buf = BitConverter.ToSingle(data, 0); break;
                    case 0x89: buf = BitConverter.ToDouble(data, 0); break;
                        //case 0x8A: buf = data[0]; break;
                        //case 0x8B: buf = BitConverter.ToUInt16(data, 0); break;
                        //case 0x8C: buf = BitConverter.ToUInt32(data, 0); break;
                        //case 0x0D: buf = data[0]; break;
                        //case 0x8E: buf = BitConverter.ToUInt64(data, 0); break;
                        //case 0x8F: buf = BitConverter.ToUInt64(data, 0); break;
                        //case 0x90: buf = BitConverter.ToUInt64(data, 0); break;
                        //default: buf = YLib.binary2HexString(data, 0, Size); break;
                }
            }
            return buf;
        }

        /// <summary>
        /// byte データを文字列に変換
        /// </summary>
        /// <param name="data">byteデータ</param>
        /// <param name="baseType">データの種別</param>
        /// <returns>文字列</returns>
        public string getDataString(byte[] data, int baseType)
        {
            string buf = "";
            if (data != null && 0 < data.Length) {
                switch (baseType) {
                    case 0x00: buf = data[0].ToString("X2"); break;
                    case 0x01: buf = data[0].ToString("X2"); break;
                    case 0x02: buf = data[0].ToString("X2"); break;
                    case 0x83: buf = BitConverter.ToInt16(data, 0).ToString("X4"); break;
                    case 0x84: buf = BitConverter.ToUInt16(data, 0).ToString("X4"); break;
                    case 0x85: buf = BitConverter.ToInt32(data, 0).ToString("X8"); break;
                    case 0x86: buf = BitConverter.ToUInt32(data, 0).ToString("X8"); break;
                    case 0x07: buf = data[0].ToString(); break;
                    case 0x88: buf = BitConverter.ToSingle(data, 0).ToString(); break;
                    case 0x89: buf = BitConverter.ToDouble(data, 0).ToString(); break;
                    case 0x8A: buf = data[0].ToString("X2"); break;
                    case 0x8B: buf = BitConverter.ToUInt16(data, 0).ToString("X4"); break;
                    case 0x8C: buf = BitConverter.ToUInt32(data, 0).ToString("X8"); break;
                    case 0x0D: buf = data[0].ToString("X2"); break;
                    case 0x8E: buf = BitConverter.ToUInt64(data, 0).ToString("X16"); break;
                    case 0x8F: buf = BitConverter.ToUInt64(data, 0).ToString("X16"); break;
                    case 0x90: buf = BitConverter.ToUInt64(data, 0).ToString("X16"); break;
                    default: buf = YLib.binary2HexString(data, 0, Size); break;
                }
            }
            return buf;
        }

        /// <summary>
        /// FITの日時データをDateTimeに変換
        /// FITの日時データは1989/12/31 00:00:00 を起点とした秒数値
        /// </summary>
        /// <param name="fitDate">FITデータ(s)</param>
        /// <returns>DateTime</returns>
        private DateTime fitDate2DateTime(uint fitDate)
        {
            var baseDate = new DateTime(1989, 12, 31, 0, 0, 0);
            return baseDate.AddSeconds(fitDate);
        }
    }

    /// <summary>
    /// Developer Field
    //  Table 8 – Developer Field Description (FIT SDK)
    /// </summary>
    public class DeveloperFieldDescription
    {
        public int FieldNumber;
        public int Size;
        public int DeveloperDataIndex;
        public byte[] Data;

        public int size;

        public DeveloperFieldDescription(byte[] data, int start)
        {
            FieldNumber = (int)data[start];
            Size = (int)data[start + 1];
            DeveloperDataIndex = (int)data[start + 2];
            size = 3;
        }

        public string toString()
        {
            string buf = $"FieldNumber:{FieldNumber} Size:{Size} DeveloperDataIndex:{DeveloperDataIndex}";
            if (Data != null && 0 < Data.Length) {
                buf += ":" + YLib.binary2HexString(Data, 0, Size);
            }
            return buf;
        }

        public string toCsvString()
        {
            if (Data != null && 0 < Data.Length) {
                return YLib.binary2HexString(Data, 0, Size);
            } else {
                return $"FieldNumber:{FieldNumber} Size:{Size} DeveloperDataIndex:{DeveloperDataIndex}";
            }
        }
    }

    /// <summary>
    /// Definitin Message データの定義
    //  Table 4. Definition Message (FIT SDK)
    /// </summary>
    class DefinitionMessge
    {
        //  Table 4. Definition Message Contents (FIT SDK)
        public RecordHeader recordHeader;       //  Record Header
        public int Architecture;                //  0:Little Endian 1:Big Endian
        public int GlobalMessageNumber;         //  データの分類(mFieldName参照)
        public int Fields;                      //  1レコードのデータ数
        public List<FieldDefinition> FieldData; //  データの属性
        //  Developer用データ(通常未使用) RecordHeader.MessageTypeSpecificで判断
        public byte DeveloperFields;
        public List<DeveloperFieldDescription> DeveloperFieldData;

        public int size = -1;                    //  このデータのサイズ(データには含まれない)
        private readonly int CRCsize = 2;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="data">データ</param>
        /// <param name="start">位置</param>
        public DefinitionMessge(byte[] data, int start)
        {
            int pos = start;
            if (data.Length <= pos + 1 + 5 + CRCsize)     //  size = NormalFeaderSize(1byte) + RecordHeaderSize(>= 5byte)
                return;
            recordHeader = new RecordHeader(data[pos]);
            pos += recordHeader.size;
            pos++;
            Architecture = data[pos];
            pos++;
            GlobalMessageNumber = BitConverter.ToInt16(data, pos);  //  = global ID
            pos += 2;
            Fields = (int)data[pos];                //  定義データ数
            pos++;

            if (FieldData == null)
                FieldData = new List<FieldDefinition>();
            FieldData.Clear();
            //  定義項目の取得
            for (int i = 0; i < Fields; i++) {
                FieldDefinition fieldDefinition = new FieldDefinition(data, pos, GlobalMessageNumber);
                FieldData.Add(fieldDefinition);
                pos += fieldDefinition.size;
                if (data.Length <= pos)
                    return;
            }
            if (recordHeader.MessageTypeSpecific) {
                //  開発者向けデータ
                DeveloperFields = data[pos];        //  データ数
                pos++;
                if (DeveloperFieldData == null)
                    DeveloperFieldData = new List<DeveloperFieldDescription>();
                DeveloperFieldData.Clear();
                //  定義項目の取得
                for (int i = 0; i < DeveloperFields; i++) {
                    DeveloperFieldDescription developerFieldDescription = new DeveloperFieldDescription(data, pos);
                    DeveloperFieldData.Add(developerFieldDescription);
                    pos += developerFieldDescription.size;
                    if (data.Length <= pos)
                        return;
                }
            }
            size = pos - start;
        }

        /// <summary>
        /// 定義データを文字列に変換
        /// </summary>
        /// <returns>文字列</returns>
        public string toString()
        {
            string buf = recordHeader.toString();
            buf += "\n" + definitionHeader();
            for (int i = 0; i < FieldData.Count; i++)
                buf += "\n" + FieldData[i].toString();
            if (recordHeader.MessageTypeSpecific) {
                buf += "\n" + $"DeveloperFields:{DeveloperFields}";
                for (int i = 0; i < DeveloperFieldData.Count; i++)
                    buf += "\n" + DeveloperFieldData[i].toString();
            }

            return buf;
        }

        /// <summary>
        /// 定義データをCSV形式に変換
        /// </summary>
        /// <returns></returns>
        public string toCsvString()
        {
            string buf = recordHeader.toString() + "\n";
            buf += definitionHeader() + "\n";
            string sp = "";
            for (int i = 0; i < FieldData.Count; i++) {
                buf += sp + FieldData[i].toCsvString();
                sp = ",";
            }
            sp = "";
            if (recordHeader.MessageTypeSpecific) {
                buf += $"\nDeveloperFields:{DeveloperFields}\n";
                for (int i = 0; i < DeveloperFieldData.Count; i++) {
                    buf += sp + DeveloperFieldData[i].toString();
                    sp = ",";
                }
            }
            return buf;
        }

        /// <summary>
        /// 定義データのヘッダを文字列に変換
        /// </summary>
        /// <returns></returns>
        private string definitionHeader()
        {
            string buf = $"Architecture:{Architecture}";
            buf += $" GlobalMessageNumber:{GlobalMessageNumber}";
            if (GlobalMessage.mFieldName.ContainsKey(GlobalMessageNumber))
                buf += $" [{GlobalMessage.mFieldName[GlobalMessageNumber]}]";
            buf += $" Fields:{Fields}";
            return buf;
        }
    }

    /// <summary>
    /// Data Message Definition Mesaageで定義されたデータ値
    /// </summary>
    class DataMessage
    {
        public RecordHeader mRrecordHeader;
        public List<FieldDefinition> mFieldDefinitions;
        List<DeveloperFieldDescription> mDeveloperFieldData;
        public int mGglobalMessageNumber;
        public int mSize = -1;
        private readonly int CRCsize = 2;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="data">byteデータ</param>
        /// <param name="start">データ位置</param>
        /// <param name="fieldsDef">定義データ</param>
        /// <param name="globalMessageNumber">Global Message Number</param>
        public DataMessage(byte[] data, int start, List<FieldDefinition> fieldsDef, List<DeveloperFieldDescription> developerFieldData, int globalMessageNumber)
        {
            int pos = start;
            if (data.Length <= pos + 1 + CRCsize)
                return;
            mRrecordHeader = new RecordHeader(data[pos]);
            pos += mRrecordHeader.size;
            if (fieldsDef != null) {
                foreach (FieldDefinition fieldDef in fieldsDef) {
                    fieldDef.Data = YLib.ByteCopy(data, pos, fieldDef.Size);
                    pos += fieldDef.Size;
                    if (data.Length <= pos)
                        return;
                }
                mFieldDefinitions = fieldsDef;
            } else {
                mFieldDefinitions = null;
            }
            this.mGglobalMessageNumber = globalMessageNumber;
            if (developerFieldData != null && 0 < developerFieldData.Count) {
                foreach (DeveloperFieldDescription fieldDef in developerFieldData) {
                    fieldDef.Data = YLib.ByteCopy(data, pos, fieldDef.Size);
                    pos += fieldDef.Size;
                    if (data.Length <= pos)
                        return;
                }
                mDeveloperFieldData = developerFieldData;
            }

            mSize = pos - start;
        }

        /// <summary>
        /// 座標データを取り出す
        /// </summary>
        /// <returns></returns>
        public Point getCoordinate()
        {
            Point p = new Point();
            foreach (FieldDefinition fieldDef in mFieldDefinitions) {
                if (mGglobalMessageNumber == 20) {
                    if (fieldDef.Data != null) {
                        Field field = GlobalMessage.getFieldName(mGglobalMessageNumber, fieldDef.FieldDefinitionNumber);
                        if (field.type == 0) {
                            p.Y = fieldDef.getDouble();
                        } else if (field.type == 1) {
                            p.X = fieldDef.getDouble();
                        }
                    }
                }
            }
            return p;
        }

        /// <summary>
        /// GPSデータを取り出す(日付、座標、標高)
        /// </summary>
        /// <returns></returns>
        public GpsData getGpsData()
        {
            GpsData p = new GpsData();
            foreach (FieldDefinition fieldDef in mFieldDefinitions) {
                if (mGglobalMessageNumber == 20) {
                    if (fieldDef.Data != null) {
                        Field field = GlobalMessage.getFieldName(mGglobalMessageNumber, fieldDef.FieldDefinitionNumber);
                        if (field.num == 0) {
                            p.mLatitude = fieldDef.getDouble();
                        } else if (field.num == 1) {
                            p.mLongitude = fieldDef.getDouble();
                        } else if (field.num == 2) {
                            p.mElevator = fieldDef.getDouble();
                        } else if (field.num == 253) {
                            p.mDateTime = fieldDef.getDateTime();
                        }
                    }
                }
            }
            return p;
        }

        /// <summary>
        /// データ値を文字列に変換
        /// </summary>
        /// <returns></returns>
        public string toString()
        {
            string buf = mRrecordHeader.toString();
            foreach (FieldDefinition fieldDef in mFieldDefinitions) {
                buf += "\n" + fieldDef.toString();
            }
            return buf;
        }

        /// <summary>
        /// データ値をCSV形式に変換
        /// </summary>
        /// <returns></returns>
        public string toCsvString()
        {
            string buf = "";
            string sp = "";
            if (mFieldDefinitions != null && 0 < mFieldDefinitions.Count) {
                foreach (FieldDefinition fieldDef in mFieldDefinitions) {
                    buf += sp + fieldDef.toCsvString();
                    sp = ",";
                }
            }
            if (mDeveloperFieldData != null && 0 < mDeveloperFieldData.Count) {
                foreach (DeveloperFieldDescription fieldDef in mDeveloperFieldData) {
                    buf += sp + fieldDef.toCsvString();
                }
            }
            return buf;
        }
    }

    /// <summary>
    /// FIT データ読込(Main)
    /// FitReader(string path)                  FITファイルを設定
    /// getDataRecordAll(DATATYPE dataType)     FITファイルを読み込む
    /// dataChk()                               エラーチェックと修正
    /// </summary>
    public class FitReader
    {
        public enum DATATYPE { non, gpxData, gpxSimpleData };
        public DATATYPE mDataType = DATATYPE.gpxData;           //  取得データの種類
        public List<GpsData> mListGpsData;                      //  GPSデータ(時間/座標/標高)[DATATYPE.gpsData]
        public List<Point> mListGpsPointData;                   //  GPS座標データリスト[DATATYPE.gpsSImpleData]
        public GpsInfoData mGpsInfoData;                        //  gpsデータ情報

        private byte[] mFitData;                                //  FIT データ
        private int mPos = 0;                                   //  読込位置
        private FileHeader mHeader;                             //  ファイルヘッダ
        private List<FieldDefinition> mDefinitionData;          //  定義データ
        private List<DeveloperFieldDescription> mDeveloperFields;   //  開発者用定義データ  
        private int mGlobalMessageNumber;                       //  データの分類
        public int mBackSize = 0;                               //  データの残量

        YLib ylib = new YLib();
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="data">Stream Data</param>
        public FitReader(byte[] data)
        {
            mFitData = data;
            if (mFitData.Length <= 12)
                return;
            mHeader = new FileHeader(mFitData);
            mPos += mHeader.size;
            mBackSize = mFitData.Length - mPos;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public FitReader(string path)
        {
            using (var src = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                byte[] data = new byte[src.Length];
                src.Read(data, 0, data.Length);
                mFitData = data;
                if (mFitData.Length <= 12)
                    return;
                mHeader = new FileHeader(mFitData);
                mPos += mHeader.size;
                mBackSize = mFitData.Length - mPos;
            }
        }

        /// <summary>
        /// ファイルヘッダを文字列に変化
        /// </summary>
        /// <returns>文字列</returns>
        public string FileHeaderString()
        {
            return mHeader.toString();
        }

        /// <summary>
        /// 定義データの取得
        /// </summary>
        /// <returns></returns>
        public bool getDefinitionMessage()
        {
            DefinitionMessge definitionMessage = new DefinitionMessge(mFitData, mPos);
            if (definitionMessage.size <= 0) {
                mBackSize = 0;
                return false;
            }
            mDefinitionData = definitionMessage.FieldData;
            mDeveloperFields = definitionMessage.DeveloperFieldData;
            mGlobalMessageNumber = definitionMessage.GlobalMessageNumber;
            mPos += definitionMessage.size;
            mBackSize = mFitData.Length - mPos;
            return true;
        }


        /// <summary>
        /// 定義データを文字列で取得
        /// </summary>
        /// <param name="csv">CSV形式</param>
        /// <returns>文字列</returns>
        public string getDefinitionMessageString(bool csv = false)
        {
            DefinitionMessge definitionMessage = new DefinitionMessge(mFitData, mPos);
            if (definitionMessage.size <= 0) {
                mBackSize = 0;
                return "";
            }
            mDefinitionData = definitionMessage.FieldData;
            mDeveloperFields = definitionMessage.DeveloperFieldData;
            mGlobalMessageNumber = definitionMessage.GlobalMessageNumber;
            mPos += definitionMessage.size;
            mBackSize = mFitData.Length - mPos;
            if (csv)
                return definitionMessage.toCsvString();
            else
                return definitionMessage.toString();
        }

        /// <summary>
        /// データ値の取得
        /// </summary>
        /// <param name="fieldDef"></param>
        /// <param name="globalMessageNumber"></param>
        /// <returns></returns>
        public bool getDataMessage(List<FieldDefinition> fieldDef, List<DeveloperFieldDescription> developerFields, int globalMessageNumber)
        {
            DataMessage dataMessage = new DataMessage(mFitData, mPos, fieldDef, developerFields, globalMessageNumber);
            if (dataMessage.mSize <= 0) {
                mBackSize = 0;
                return false;
            }
            mPos += dataMessage.mSize;
            mBackSize = mFitData.Length - mPos;

            //  GPSデータの取得
            if (mGlobalMessageNumber == 20) {
                //  Record Data
                if (mDataType == DATATYPE.gpxData) {
                    GpsData gpxData = dataMessage.getGpsData();
                    mListGpsData.Add(gpxData);
                } else if (mDataType == DATATYPE.gpxSimpleData) {
                    Point coord = dataMessage.getCoordinate();
                    mListGpsPointData.Add(coord);
                }
            }

            return true;
        }

        /// <summary>
        /// データ値を文字列で取得
        /// </summary>
        /// <param name="fieldDef"></param>
        /// <param name="globalMessageNumber"></param>
        /// <param name="csv"></param>
        /// <returns></returns>
        public string getDataMessageString(List<FieldDefinition> fieldDef, List<DeveloperFieldDescription> developerFields, int globalMessageNumber, bool csv = false)
        {
            DataMessage dataMessage = new DataMessage(mFitData, mPos, fieldDef, developerFields, globalMessageNumber);
            if (dataMessage.mSize <= 0) {
                mBackSize = 0;
                return "";
            }
            mPos += dataMessage.mSize;
            mBackSize = mFitData.Length - mPos;
            if (csv)
                return dataMessage.toCsvString();
            else
                return dataMessage.toString();
        }

        /// <summary>
        /// データレコードを文字列で取得
        /// </summary>
        /// <param name="csv">CSV形式</param>
        /// <returns>文字列</returns>
        public string getDataRecordSting(bool csv = false)
        {
            if (mBackSize <= 2) {
                mBackSize = 0;
                byte[] crc = YLib.ByteCopy(mFitData, mPos, 2);
                return "CRC:[0x" + BitConverter.ToUInt16(mFitData, mPos).ToString("X2") + "]";
            }
            RecordHeader normalHeader = new RecordHeader(mFitData[mPos]);
            if (normalHeader.size <= 0) {
                mBackSize = 0;
                return "";
            }
            if (normalHeader.MessageType == 1) {
                return (csv ? "" : "\n") + "\n" + getDefinitionMessageString(csv);
            } else {
                return (csv ? "" : "\n") + getDataMessageString(mDefinitionData, mDeveloperFields, mGlobalMessageNumber, csv);
            }
        }

        /// <summary>
        /// GPSデータを取得する
        /// </summary>
        /// <returns></returns>
        public int getDataRecordAll(DATATYPE dataType)
        {
            mDataType = dataType;
            if (mDataType == DATATYPE.gpxData) {
                if (mListGpsData == null)
                    mListGpsData = new List<GpsData>();
            } else if (mDataType == DATATYPE.gpxSimpleData) {
                if (mListGpsPointData == null)
                    mListGpsPointData = new List<Point>();
                if (mGpsInfoData == null)
                    mGpsInfoData = new GpsInfoData();
            } else
                return 0;
            //  データの取得
            do {
                RecordHeader normalHeader = new RecordHeader(mFitData[mPos]);
                if (normalHeader.size <= 0) {
                    mBackSize = 0;
                    break;
                }
                if (normalHeader.MessageType == 1) {
                    getDefinitionMessage();
                } else {
                    getDataMessage(mDefinitionData, mDeveloperFields, mGlobalMessageNumber);
                }
            } while (0 < mBackSize);
            //  GPS情報の設定
            if (mDataType == DATATYPE.gpxData) {
                mGpsInfoData = getGpsInfoData();
                mListGpsPointData = mListGpsData.ConvertAll(p => new Point(p.mLongitude, p.mLatitude));
                return mListGpsData.Count;
            } else if (mDataType == DATATYPE.gpxSimpleData) {
                return mListGpsPointData.Count;
            }
            return 0;
        }

        /// <summary>
        /// GPS情報の取得
        /// </summary>
        /// <returns></returns>
        public GpsInfoData getGpsInfoData()
        {
            if (mListGpsData != null) {
                GpsInfoData gpsInfoData = new GpsInfoData();
                gpsInfoData.setData(mListGpsData);
                return gpsInfoData;
            }
            return null;
        }

        /// <summary>
        /// GPS情報(概要)を文字列に変換
        /// </summary>
        /// <returns></returns>
        public string getGpsInfoDataString()
        {
            if (mListGpsData != null) {
                GpsInfoData gpsInfoData = new GpsInfoData();
                gpsInfoData.setData(mListGpsData);
                return gpsInfoData.toString();
            }
            return "";
        }

        /// <summary>
        /// エラーデータをチェックし問題があれば削除する
        /// データ間で1km以上離れている点を除外、mGpsInfoも再取得
        /// </summary>
        public void dataChk()
        {
            if (mDataType == DATATYPE.gpxData) {
                if (2 < mListGpsData.Count) {
                    List<GpsData> gpsDataList = new List<GpsData>();
                    double dis0 = mListGpsData[0].distance(mListGpsData[1]);
                    double dis1 = mListGpsData[0].distance(mListGpsData[2]);
                    if (dis0 < 1.0 || dis1 < 1.0)
                        gpsDataList.Add(mListGpsData[0]);
                    for (int i = 1; i < mListGpsData.Count - 1; i++) {
                        dis1 = mListGpsData[i].distance(mListGpsData[i + 1]);
                        if (dis0 < 1.0 || dis1 < 1.0)
                            gpsDataList.Add(mListGpsData[i]);
                        dis0 = dis1;
                    }
                    int n = mListGpsData.Count - 1;
                    dis0 = mListGpsData[n].distance(mListGpsData[n - 1]);
                    dis1 = mListGpsData[n].distance(mListGpsData[n - 2]);
                    if (dis0 < 1.0 || dis1 < 1.0)
                        gpsDataList.Add(mListGpsData[n]);
                    mListGpsData = new List<GpsData>(gpsDataList);
                    mGpsInfoData.setData(mListGpsData);
                }
            } else if (mDataType == DATATYPE.gpxSimpleData) {
                if (2 < mListGpsPointData.Count) {
                    List<Point> gpsPointList = new List<Point>();
                    int n = 1;
                    double dis0 = ylib.coordinateDistance(mListGpsPointData[0], mListGpsPointData[1]);
                    double dis1 = ylib.coordinateDistance(mListGpsPointData[1], mListGpsPointData[2]);
                    if (dis0 < 1.0 || dis1 < 1.0)
                        gpsPointList.Add(mListGpsPointData[0]);
                    for (int i = 1; i < mListGpsPointData.Count - 1; i++) {
                        dis1 = ylib.coordinateDistance(mListGpsPointData[i], mListGpsPointData[i + 1]);
                        if (dis0 < 1.0 || dis1 < 1.0)
                            gpsPointList.Add(mListGpsPointData[i]);
                        dis0 = dis1;
                    }
                    n = mListGpsPointData.Count - 1;
                    dis0 = ylib.coordinateDistance(mListGpsPointData[n], mListGpsPointData[n - 1]);
                    dis1 = ylib.coordinateDistance(mListGpsPointData[n], mListGpsPointData[n - 2]);
                    if (dis0 < 1.0 || dis1 < 1.0)
                        gpsPointList.Add(mListGpsPointData[n]);
                    mListGpsPointData = new List<Point>(gpsPointList);
                    mGpsInfoData.setTotalDistance(mListGpsPointData);
                    mGpsInfoData.setArea(mListGpsPointData);

                }
            }
        }

        /// <summary>
        /// 時刻から座標を取得する
        /// </summary>
        /// <param name="datetime">日時</param>
        /// <returns>座標</returns>
        public Point getCoordinate(DateTime datetime)
        {
            Point pos = new Point();
            if (datetime < mListGpsData[0].mDateTime ||
                mListGpsData[mListGpsData.Count - 1].mDateTime < datetime)
                return pos;
            for (int i = 0; i < mListGpsData.Count - 1; i++) {
                if (datetime < mListGpsData[i].mDateTime) {
                    double scale = (double)datetime.Subtract(mListGpsData[i - 1].mDateTime).Ticks
                        / (double)mListGpsData[i].mDateTime.Subtract(mListGpsData[i - 1].mDateTime).Ticks;
                    Point v = mListGpsData[i - 1].getCoordinate().vector(mListGpsData[i].getCoordinate());
                    v.scale(scale);
                    pos = mListGpsData[i].getCoordinate().add(v);
                    break;
                }
            }
            return pos;
        }
    }
}
