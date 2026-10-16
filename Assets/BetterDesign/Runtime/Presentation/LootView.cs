using BetterDesign.Core;
using UnityEngine;

namespace BetterDesign
{
    // Scene-authored renderer. Loot quantity, ownership and pickup rules live in the controller/model.
    public sealed class LootView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite goldIcon,supplyIcon,mushroomIcon;
        public bool IsConfigured { get { return spriteRenderer != null && goldIcon != null && supplyIcon != null && mushroomIcon != null; } }
        public void Show(ItemId item)
        {
            spriteRenderer.sprite = item == ItemId.Gold ? goldIcon : item == ItemId.Supply ? supplyIcon : mushroomIcon;
            gameObject.SetActive(true);
        }
        public void Render(float x,float y,float bob)
        {
            transform.position = new Vector3(x,y + bob,0);
            spriteRenderer.sortingOrder = 180 - Mathf.RoundToInt(y*10);
        }
        public void Hide() { if (this != null) gameObject.SetActive(false); }
    }
}
