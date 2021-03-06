﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartMoveWebApp.Models.ViewModels
{
    public class EditDriverProfileViewModel
    {
        public IEnumerable<TruckType> TruckTypesList;

        [Required]
        public int TruckOwnerId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(12)]
        [RegularExpression("\\d{3}-\\d{3}-\\d{4}", ErrorMessage = "Phone Number should be of 10 digits only")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "Address Line 1")]
        public string Address1 { get; set; }

        [Display(Name = "Address Line 2")]
        public string Address2 { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Zip Code should be of maximum 10 charcters.")]
        [DataType(DataType.PostalCode)]
        [RegularExpression("^\\d{5}(?:[-]\\d{4})?$", ErrorMessage = "Zip Code should be in a valid format.")]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        [StringLength(50)]
        public string State { get; set; }

        [Required(ErrorMessage = "Truck Type field is required.")]
        [Display(Name = "Truck Type")]
        public int TruckTypeId { get; set; }

        public IEnumerable<SelectListItem> TruckTypes
        {
            get
            {
                var allTruckTypes = TruckTypesList.Select(f => new SelectListItem
                {
                    Value = f.TruckTypeId.ToString(),
                    Text = f.Type
                });
                return DefaultTruckType.Concat(allTruckTypes);
            }
        }

        public IEnumerable<SelectListItem> DefaultTruckType
        {
            get
            {
                return Enumerable.Repeat(new SelectListItem
                {
                    Value = "",
                    Text = "Select a Truck Type"
                }, count: 1);
            }
        }

        [Required]
        [StringLength(50)]
        [Display(Name = "Truck License Plate")]
        public string LicensePlate { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Truck Color")]
        public string TruckColor { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Truck Make")]
        public string TruckMake { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Truck Model")]
        public string TruckModel { get; set; }

        [Required]
        [Range(1980, 2018, ErrorMessage = "Please enter a year between 1980 and 2018.")]
        [Display(Name = "Truck Year")]
        public int? TruckYear { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Driver License Number")]
        public string DriverLicenseNumber { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Vehicle Registration Number")]
        public string VehicleRegNumber { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Driver Insurnace Policy Number")]
        public string DriverInsurancePolicy { get; set; }

        [Display(Name = "Select Profile Picture")]
        public string ProfilePictureURL { get; set; }

        public double AverageRating { get; set; }
    }
}