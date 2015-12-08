using UnityEngine;

// 씬을 넘어 사용하고 싶은 파라미터.
public class GlobalParam : MonoBehaviour {
	
	public	int			global_account_id	= 0;			// 글로벌 고유 어카운트 id.

	public	bool		is_host				= false;		// 호스트로서 플레이하고 있는가?.

	private static		GlobalParam instance = null;

    public int          playerNum = 0;

	public bool			fadein_start = false;

	public bool[]		is_connected = new bool[NetConfig.PLAYER_MAX];

	// 통신에서 사용하는 난수 시드 값.
	public int			seed = 0;

	// ================================================================ //
	
	public void		create()
	{
		for(int i = 0;i < this.is_connected.Length;i++) {

			this.is_connected[i] = false;
		}
	}

	// ================================================================ //

    void Awake()
    {
        if (instance == null)
        {
            GameObject go = GameObject.Find("GlobalParam");

            instance = go.GetComponent<GlobalParam>();
            instance.create();

            DontDestroyOnLoad(go);
        }
    }


	public static GlobalParam	get()
    {
        return (instance);
	}
}
