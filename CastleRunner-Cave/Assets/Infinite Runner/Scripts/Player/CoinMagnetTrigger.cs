using UnityEngine;
using System.Collections;

/*
 * The coin magnet trigger will be active when the power up is active, and it increases the radius that collects coins
 */
public class CoinMagnetTrigger : MonoBehaviour {
	
	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Coin")) {
			other.GetComponent<CoinObject>().collectCoin();
		}
	}
}
