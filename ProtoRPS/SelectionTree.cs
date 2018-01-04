namespace ProtoRPS
{
    using System;
    using System.Collections.Generic;


    /// <summary>
    /// 
    /// Selects weighted random values from a collection.  Also provides methods to
    /// modify the relative weights associated with the values in the collection.
    /// 
    /// </summary>
    /// <typeparam name="Type">The type of values the selector is able to return</typeparam>

    class Selector<Type>
    {

        /// <summary>
        /// 
        /// Represents a value the Selector is capable of returning, along with the relative
        /// weight associated with that value
        /// 
        /// </summary>

        private class Category
        {
            public int Weight { get; set; }
            public Type Value { get; }

            public Category(int weight, Type value)
            {
                Weight = weight;
                Value = value;
            }
        }


        private Random randomizer;
        private List<Category> categories;
        private int totalWeight;


        public Selector()
        {
            randomizer = new Random();
            categories = new List<Category>();
            totalWeight = 0;
        }


        public Selector(Random randomizer)
        {
            GiveRandomizer(randomizer);
            categories = new List<Category>();
            totalWeight = 0;
        }

        /// <summary>
        /// 
        /// Passes an instance of the Random class to this
        /// 
        /// </summary>
        /// <param name="randomizer">The instance of the Random class for this to use</param>
        
        public void GiveRandomizer(Random randomizer)
        {
            this.randomizer = randomizer;
        }


        /// <summary>
        /// 
        /// Adds a value to the Selector's possible results
        /// 
        /// </summary>
        /// <param name="weight">The initial relative frequency at which the new value is chosen</param>
        /// <param name="value">The new value to add</param>
        
        public void AddCategory(int weight, Type value)
        {
            // Prevent addition of duplicate categories
            foreach (Category category in categories)
                if (category.Value.Equals(value))
                    return;

            categories.Add(new Category(weight, value));
            totalWeight += weight;
        }

        /// <summary>
        /// 
        /// Randomly selects and returns one of the values in categories
        /// 
        /// </summary>
        /// <returns>The selected value</returns>

        public Type Pick()
        {
            if (randomizer == null)
                throw new InvalidOperationException("No Random class instance found.");
            if (categories.Count == 0)
                throw new InvalidOperationException("Selector has no values to pick from.");

            int random = (int)randomizer.Next() % totalWeight;
            foreach (Category category in categories)
            {
                random -= category.Weight;
                if (random < 0)
                    return category.Value;
            }

            throw new InvalidOperationException("Pick function failed to pick a value.");
        }


        /// <summary>
        /// 
        /// Increases the relative frequency at which the Selector chooses the passed value
        /// 
        /// </summary>
        /// <param name="goodValue">The value which should be selected more frequently</param>

        public void Reward(Type goodValue)
        {
            // Get the category representing the move to be rewarded
            Category goodCategory = null;
            foreach (Category category in categories)
                if (category.Value.Equals(goodValue))
                {
                    goodCategory = category;
                    break;
                }

            // Return if goodMove doesn't exist in the selection tree
            if (goodCategory == null)
                return;

            // Move 1 weight to goodCategory from each other category, if available
            foreach (Category category in categories)
                if (category != goodCategory && category.Weight > 0)
                {
                    category.Weight--;
                    goodCategory.Weight++;
                }
        }


        /// <summary>
        /// 
        /// Decreases the relative frequency at which the Selector chooses the passed value
        /// 
        /// </summary>
        /// <param name="badValue">The value which should be selected less frequently</param>

        public void Punish(Type badValue)
        {
            // Get the category representing the move to be punished
            Category badCategory = null;
            foreach (Category category in categories)
                if (category.Value.Equals(badValue))
                {
                    badCategory = category;
                    break;
                }

            // Return if badMove doesn't exist in the selection tree
            if (badCategory == null)
                return;

            // Move 1 weight from badCategory to each other category, if available
            foreach (Category category in categories)
                if (category != badCategory && badCategory.Weight > 0)
                {
                    category.Weight++;
                    badCategory.Weight--;
                }
        }
     }
}
