/// <summary>
/// SendTypeを元に何が送られてきてるかを判断する
/// </summary>
public enum SendType
{
    None,
    PositionSender,
    SettingSender,
    Intensity,
    IIntensities,
    ReCalcData,
    SpatialMap,
    SpatialMesh,
    DeleteData,
    ReproData
}