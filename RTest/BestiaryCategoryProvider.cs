﻿using RecipaediaEX;

namespace RTest {
    public class BestiaryCategoryProvider : IRecipaediaCategoryProvider {
        public IEnumerable<IRecipaediaCategory> GetCategories() {
            yield return new BestiaryCategory();
        }
    }
}