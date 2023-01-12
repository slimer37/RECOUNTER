using UnityEngine;

namespace Recounter.Tablet
{
    public class PageGroup : MonoBehaviour
    {
        Page[] _pages;

        void Awake()
        {
            _pages = GetComponentsInChildren<Page>();
            _pages[0].Open();
        }

        public void CloseOtherPages(Page excludePage)
        {
            foreach (var page in _pages)
            {
                if (page != excludePage)
                    page.Close();
            }
        }
    }
}