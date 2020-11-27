/// <summary>
/// SendTypeを元に何が送られてきてるかを判断する
/// </summary>
public enum SendType
{
    None,
    MeasurementType,
    PositionSender,
    SettingSender,
    Intensity,
    IIntensities,
    ReCalcData,
    ReCalcTransData,
    SpatialMap,
    SpatialMesh,
    DeleteData,
    ReproData
}

/// <summary>
/// Server側は接続時に計測方法(時間平均standard or 過渡音計測Transient)をClientに通達する
/// </summary>
public enum MeasurementType
{
    Standard,
    Transient
}