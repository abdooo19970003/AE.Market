using System;
using System.Collections.Generic;
using System.Text;
using AE.Market.Domain.Aggregates.Catalog.Products;

namespace AE.Market.Infrastructure.Persistence.Seeders
{
    public static class ProductTaxCodeSeeder
    {
        public static IEnumerable<ProductTaxCode> GetSeedData()
        {
            return new List<ProductTaxCode>
            {
                // ─────────────────────────────────────────────
                // GENERAL / CATCH-ALL
                // ─────────────────────────────────────────────

                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_99999999",
                    "physical",
                    null,
                    "General - Tangible Goods",
                    "A physical good that can be moved or touched. Also known as tangible personal property."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_00000000",
                    "physical",
                    null,
                    "Nontaxable",
                    "Any nontaxable good or service which can be used to ensure no tax is applied, even for jurisdictions that would otherwise tax it."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_20030000",
                    "services",
                    null,
                    "General - Services",
                    "General category for services. Only use this if you don't have a more specific category."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_10000000",
                    "digital",
                    null,
                    "General - Electronically Supplied Services",
                    "A digital service provided mainly through the internet with minimal human involvement, relying on information technology."
                ),
                // ─────────────────────────────────────────────
                // CLOTHING & FASHION
                // ─────────────────────────────────────────────

                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_30011000",
                    "physical",
                    null,
                    "Clothing & Footwear",
                    "Apparel and footwear for people made for general use."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_30011200",
                    "physical",
                    null,
                    "Children's Clothing and Footwear",
                    "Children's clothing and footwear including general purpose articles intended to be worn by a person under 18 years of age."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_30021000",
                    "physical",
                    null,
                    "Athletic Activity Clothing",
                    "Clothing, footwear, and accessories worn on a person's body while participating in recreational or sporting activities."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_30060001",
                    "physical",
                    null,
                    "Purses and Handbags",
                    "Bags including handbags, purses, coin purses, fanny packs / bum bags, and diaper / nappy bags."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_30060007",
                    "physical",
                    null,
                    "Jewelry",
                    "Jewelry such as necklaces, earrings, rings, and more."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_30060015",
                    "physical",
                    null,
                    "Luggage",
                    "Suitcases and baggage typically used for transporting travellers' belongings."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_30060016",
                    "physical",
                    null,
                    "Watches",
                    "A small timepiece worn typically on a strap on one's wrist."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_30060017",
                    "physical",
                    null,
                    "Sunglasses - Non-prescription",
                    "Sunglasses with a lens containing no lens powers to correct vision problems."
                ),
                // ─────────────────────────────────────────────
                // ELECTRONICS & TECHNOLOGY
                // ─────────────────────────────────────────────

                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_34020027",
                    "physical",
                    null,
                    "Consumer Electronics",
                    "Electronic devices bought for personal rather than commercial use. Please select a more granular product code if one is available."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_34021000",
                    "physical",
                    null,
                    "Mobile Phones",
                    "A portable device for connecting to a telecommunications network in order to transmit and receive voice and data communications."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_34020006",
                    "physical",
                    null,
                    "Televisions",
                    "Televisions."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_34020004",
                    "physical",
                    null,
                    "Headphones/Earbuds",
                    "Wired and bluetooth headphones and earbuds for audio listening."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_34020001",
                    "physical",
                    null,
                    "Digital Cameras",
                    "A camera that captures photographs in digital memory."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_37010000",
                    "physical",
                    null,
                    "Personal Computers",
                    "Personal computers, including laptops, tablets, desktops."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_34040003",
                    "physical",
                    null,
                    "Computer Drives",
                    "Storage drives, hard drives, Zip drives, and so on."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_34040006",
                    "physical",
                    null,
                    "Computer Monitor/Displays",
                    "Computer Monitor/Displays."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_34040008",
                    "physical",
                    null,
                    "Computer Printer",
                    "Computer Printer."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_34022000",
                    "physical",
                    null,
                    "Video Gaming Console - Fixed",
                    "A specialized computer system designed for interactive video gameplay that typically connects to a television or external display."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_34022001",
                    "physical",
                    null,
                    "Video Gaming Console - Portable",
                    "A handheld portable electronic device used for playing video games that has its own screen, speakers, and controls."
                ),
                // ─────────────────────────────────────────────
                // BOOKS & PHYSICAL MEDIA
                // ─────────────────────────────────────────────

                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_35010000",
                    "physical",
                    null,
                    "Books",
                    "Books or booklets consisting of text or illustrations bound in a stiffer cover than the pages."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_35010001",
                    "physical",
                    null,
                    "Books for Children",
                    "Children's books including picture books, painting, drawing, and activity books."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_35020100",
                    "physical",
                    null,
                    "Newspapers",
                    "Publications distributed to the public at regular intervals that contains news of general interest."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_35020200",
                    "physical",
                    null,
                    "Periodicals",
                    "Publications, including magazines, distributed on a periodic basis."
                ),
                // ─────────────────────────────────────────────
                // DIGITAL PRODUCTS
                // ─────────────────────────────────────────────

                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_10301000",
                    "digital",
                    null,
                    "Audiobook",
                    "The recording of a book read aloud and sold with unlimited usage (for example, a downloaded audio copy)."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_10302000",
                    "digital",
                    null,
                    "Digital Books - Downloaded - Non Subscription - Permanent Rights",
                    "Works that are generally recognized in the ordinary and usual sense as books and are transferred electronically with permanent usage rights."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_10401000",
                    "digital",
                    null,
                    "Digital Audio Works - Streamed - Non Subscription",
                    "Works that result from the fixation of a series of musical, spoken, or other sounds that are transferred electronically via streaming."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_10402000",
                    "digital",
                    null,
                    "Digital Audio Visual Works - Streamed - Non Subscription",
                    "A series of related images which, when shown in succession, impart an impression of motion, together with accompanying sounds streamed electronically."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_10201000",
                    "digital",
                    null,
                    "Video Games - Downloaded - Non Subscription - Permanent Rights",
                    "Video or electronic games transferred electronically with permanent usage rights and no subscription required."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_10202000",
                    "digital",
                    null,
                    "Downloadable Software - Personal Use",
                    "Prewritten (\"canned\") software that the buyer downloads, intended for personal use."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_10202003",
                    "digital",
                    null,
                    "Downloadable Software - Business Use",
                    "Prewritten (\"canned\") software that the buyer downloads, intended for use by a commercial entity."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_10103000",
                    "digital",
                    null,
                    "Software as a Service (SaaS) - Personal Use",
                    "Cloud services software delivered over the internet. The software isn't customized for a specific business and is intended for personal use."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_10103001",
                    "digital",
                    null,
                    "Software as a Service (SaaS) - Business Use",
                    "Cloud services software delivered over the internet. The software isn't customized for a specific business and is intended for commercial use."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_10502000",
                    "digital",
                    null,
                    "Gift Card",
                    "Gift card or gift certificate that you purchase and receive electronically, assumed to be a multi-purpose instrument redeemable for goods or services."
                ),
                // ─────────────────────────────────────────────
                // FOOD & GROCERY
                // ─────────────────────────────────────────────

                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_40040000",
                    "physical",
                    null,
                    "Food for Non-Immediate Consumption",
                    "Food and beverage products sold at retail grocery-type establishments that are intended for consumption off premises."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_40060003",
                    "physical",
                    null,
                    "Food for Immediate Consumption",
                    "Food and beverage products in a form suited for consumption on the premises of the vendor."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_40040013",
                    "physical",
                    null,
                    "Meat and Meat Products",
                    "The flesh (muscle tissue) of an animal consumed as food."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_40040019",
                    "physical",
                    null,
                    "Vegetables",
                    "Vegetables are parts of plants that are consumed as food."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_40040021",
                    "physical",
                    null,
                    "Plain Breads",
                    "Plain breads include ordinary loaves such as sourdough, multigrain, and rye loaves."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_40090001",
                    "physical",
                    null,
                    "Dietary Supplements",
                    "Tablets, capsules, powders, softgels, gelcaps, or liquids intended to supplement the diet with vitamins, minerals, herbs, or other substances."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_40100001",
                    "physical",
                    null,
                    "Candy",
                    "A preparation of natural or artificial sweeteners in combination with chocolate, fruits, nuts, or other ingredients."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_41030001",
                    "physical",
                    null,
                    "Bottled Water",
                    "Regular, unsweetened, non-carbonated water sold in containers."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_41060006",
                    "physical",
                    null,
                    "Milk, Coffee, Tea, Cocoa Beverages",
                    "Milk or milk substitutes, or drinks with bases of milk, coffee, unsweetened tea or cocoa."
                ),
                // ─────────────────────────────────────────────
                // HEALTH & PERSONAL CARE
                // ─────────────────────────────────────────────

                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_32020001",
                    "physical",
                    null,
                    "Prescription Drugs",
                    "A substance that can only be obtained via a prescription of a licensed professional, intended for diagnosis, cure, treatment, or prevention of disease."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_32020002",
                    "physical",
                    null,
                    "Drugs - Over the Counter",
                    "A drug compound or preparation that can be obtained without a prescription, intended for self-treatment of common ailments."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_32050006",
                    "physical",
                    null,
                    "Grooming and Hygiene Products",
                    "Soaps, cleaning solutions, shampoo, toothpaste, mouthwash, antiperspirants, suntan lotions, and similar personal care products."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_32050018",
                    "physical",
                    null,
                    "Toothbrush",
                    "A small brush with a long handle, used for cleaning the teeth."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_32050019",
                    "physical",
                    null,
                    "Toothpaste",
                    "A thick, soft, moist substance used on a brush for cleaning one's teeth."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_32050025",
                    "physical",
                    null,
                    "Cosmetics - Beautifying",
                    "Articles intended to be applied to the human body for cleansing, beautifying, promoting attractiveness, or altering the appearance."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_32050036",
                    "physical",
                    null,
                    "Shampoo",
                    "A hair care product for cleansing the hair and scalp."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_32050040",
                    "physical",
                    null,
                    "Sunscreen",
                    "A lotion, spray, gel, foam, stick or other topical product that absorbs or reflects some of the sun's ultraviolet radiation."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_32060001",
                    "physical",
                    null,
                    "Durable Medical Equipment for Home Use",
                    "Equipment that can withstand repeated use, is primarily and customarily used to serve a medical purpose, and is generally not useful in the absence of illness or injury."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_32070018",
                    "physical",
                    null,
                    "First Aid Kits",
                    "A collection of mixed supplies and equipment used to give basic medical treatment, often housed in a portable container."
                ),
                // ─────────────────────────────────────────────
                // BABY & KIDS
                // ─────────────────────────────────────────────

                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_31020001",
                    "physical",
                    null,
                    "Bibs",
                    "A piece of cloth or plastic fastened around a baby's neck to keep their clothes clean while eating."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_31030003",
                    "physical",
                    null,
                    "Baby Wipes",
                    "Non-medicated disposable moistened cleansing wipes for infant care."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_31031202",
                    "physical",
                    null,
                    "Infant Diapers",
                    "Disposable diapers for infants."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_31040001",
                    "physical",
                    null,
                    "Baby Feeding Bottles/Nipples",
                    "A bottle with a teat or nipple made for babies to drink from."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_31070001",
                    "physical",
                    null,
                    "Infant/Child Car Seat",
                    "A portable seat installed in motor vehicles designed to protect infants and young children in the event of a collision."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_40040004",
                    "physical",
                    null,
                    "Baby Food",
                    "Foods and formulas meant for feeding babies and infants."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_40090005",
                    "physical",
                    null,
                    "Infant Formula",
                    "A food intended for special dietary use solely as a food for infants by reason of their physical or physiological condition."
                ),
                // ─────────────────────────────────────────────
                // HOME & GARDEN
                // ─────────────────────────────────────────────

                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_33080300",
                    "physical",
                    null,
                    "Household Linens/Bedding/Towels/Shower Curtains",
                    "Includes blankets, pillows, bed linens/sheets, comforters, towels, wash cloths, shower curtains and similar household textile products."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_33080301",
                    "physical",
                    null,
                    "Bedding",
                    "Bedclothes items including sheets, pillow cases, bedspreads, comforters, blankets, throws, duvet covers, and similar items."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_33080303",
                    "physical",
                    null,
                    "Bath Towels",
                    "Towels used for individual drying of persons, including bath towels, beach towels, wash cloths, hand towels, and similar items."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_33020009",
                    "physical",
                    null,
                    "Refrigerators - Energy Star",
                    "Domestic standard size refrigerators carrying Energy Star rating."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_33020010",
                    "physical",
                    null,
                    "Dishwashers - Energy Star",
                    "Domestic dish washing appliances carrying Energy Star rating."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_33020012",
                    "physical",
                    null,
                    "Clothes Washing Machine - Energy Star",
                    "Domestic clothes washing appliances carrying Energy Star rating."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_33110005",
                    "physical",
                    null,
                    "Candles",
                    "Candles to be used as a light source."
                ),
                // ─────────────────────────────────────────────
                // SPORTS & OUTDOORS
                // ─────────────────────────────────────────────

                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_30071000",
                    "physical",
                    null,
                    "Sports Activities Equipment",
                    "Protective gear worn about the human body while participating in athletic, recreational, or sporting activities."
                ),
                // ─────────────────────────────────────────────
                // ORDER ADD-ONS / FULFILLMENT
                // ─────────────────────────────────────────────

                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_92010000",
                    "physical",
                    null,
                    "Shipping and Handling Combined Charge",
                    "A single charge that combines the delivery cost and any preparation costs for physical goods."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_92010001",
                    "physical",
                    null,
                    "Shipping",
                    "A shipping charge for the delivery of physical goods in conjunction with the sale of these goods."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_92010004",
                    "physical",
                    null,
                    "Handling Charge",
                    "A charge for handling or preparation of goods prior to shipment."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_20090010",
                    "services",
                    null,
                    "Gift Wrapping - In Conjunction with Purchase",
                    "A charge for the wrapping of articles in a box or bag with paper and other decorative additions, billed alongside the product purchase."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_20090015",
                    "services",
                    null,
                    "Warranty - Mandatory",
                    "A charge separately stated from the sale of the product itself that entitles the purchaser to future repair or replacement, included as a mandatory add-on."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_20090018",
                    "services",
                    null,
                    "Warranty - Optional",
                    "A charge separately stated from the sale of the product itself that entitles the purchaser to future repair or replacement, offered as an optional add-on."
                ),
                ProductTaxCode.Create(
                    Guid.NewGuid(),
                    "txcd_90000001",
                    "physical",
                    null,
                    "Cash Donation",
                    "A monetary donation for a cause, in which the donee receives nothing in return."
                ),
            };
        }
    }
}
