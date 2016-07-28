namespace LegendsOfDescent
{
    using System;

    public class DescriptionAttribute : Attribute
    {
        public string Description { get; private set; }

        public DescriptionAttribute(string description)
        {
            this.Description = description;
        }
    }
}
