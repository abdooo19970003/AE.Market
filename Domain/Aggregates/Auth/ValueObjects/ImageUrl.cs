using AE.Market.Domain.Common;
using AE.Market.Domain.Common.DomainErrors;

namespace AE.Market.Domain.Aggregates.Auth.ValueObjects
{
    public record ImageUrl
    {
        public string Value { get; }
 

        private ImageUrl(string value)
        {
            Value = value;
        }

        public static ImageUrl Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !Uri.TryCreate(value, UriKind.Absolute, out _))

                throw Exceptions.Profile.InvalidImageUrl;

            return new ImageUrl(value);
        }
        public static implicit operator ImageUrl(string value) => Create(value);
        public static implicit operator string(ImageUrl obj) => obj.Value;

    }
}
