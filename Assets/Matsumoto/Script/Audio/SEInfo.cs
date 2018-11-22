
/// <summary>
/// 再生されているSEの情報
/// </summary>
public class SEInfo {

	public int Index;
    public float CurTime;
    public float Volume;

	public SEInfo(int index, float curTime, float volume) {
		Index = index;
		CurTime = curTime;
		Volume = volume;
	}
}
