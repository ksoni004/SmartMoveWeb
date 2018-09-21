﻿using SmartMoveWebApp.BusniessLogic;
using SmartMoveWebApp.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SmartMoveWebApp.Dtos;
using SmartMoveWebApp.Models;
using System.Text.RegularExpressions;

namespace SmartMoveWebApp.Controllers
{
    public class TruckOwnersController : Controller
    {
        public SmartMoveEntities _context { get; set; }

        public TruckOwnersController()
        {
            _context = new SmartMoveEntities();
        }

        [CheckDriverAuthorization]
        public ActionResult Index()
        {
            string emailId = this.HttpContext.Session["DriverID"].ToString();

            var truckOwner = _context.TruckOwners.Single(t => t.Email == emailId);
            return View(truckOwner);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnURL)
        {
            var driverInfo = new LoginViewModel();

            EnsureLoggedOut();
            driverInfo.ReturnURL = returnURL;

            return View(driverInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            string OldHASHValue = string.Empty;
            byte[] SALT = new byte[64];

            if (ModelState.IsValid)
            {
                var truckOwner = _context.TruckOwners.SingleOrDefault(t => t.Email == model.Email);
                if (truckOwner == null)
                {
                    ModelState.AddModelError("Email", "Given Email is not registered with us.");
                    return View(model);
                }

                var login = _context.Logins.SingleOrDefault(l => l.Email == truckOwner.Email);
                if (login == null)
                {
                    ModelState.AddModelError("Email", "Given Email is not registered with us.");
                    return View(model);
                }
                else if (!login.EmailActivated)
                {
                    ModelState.AddModelError("Email", "Email is not verified, please verify from the email sent.");
                    return View(model);
                }

                OldHASHValue = login.Password;
                SALT = login.PasswordSalt;

                bool isValidLogin = AuthenticationLogic.CompareHashValue(model.Password, model.Email, OldHASHValue, SALT);

                if (!isValidLogin)
                {
                    ModelState.AddModelError("Password", "Given password is incorrect.");
                    return View(model);
                }

                FormsAuthentication.SignOut();
                // Write the authentication cookie  
                FormsAuthentication.SetAuthCookie(truckOwner.Email, false);

                Session["DriverID"] = model.Email;

                return RedirectToLocal(model.ReturnURL);
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            // First we clean the authentication ticket like always  
            FormsAuthentication.SignOut();

            // Second we clear the principal to ensure the user does not retain any authentication  
            HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);

            Session.Clear();
            System.Web.HttpContext.Current.Session.RemoveAll();

            // Last we redirect to a controller/action that requires authentication to ensure a redirect takes place 
            // this clears the Request.IsAuthenticated flag since this triggers a new request  
            return RedirectToLocal();
        }

        private ActionResult RedirectToLocal(string returnURL = "")
        {
            try
            {
                // If the return url starts with a slash "/" we assume it belongs to our site  
                // so we will redirect to this "action"  
                if (!string.IsNullOrWhiteSpace(returnURL) && Url.IsLocalUrl(returnURL))
                    return RedirectToAction(returnURL);

                // If we cannot verify if the url is local to our host we redirect to a default location  
                return RedirectToAction("Dashboard", "TruckOwners");
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterDriver(RegisterDriverViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Unqieness validations
                var checkEmailUniqueness = _context.Logins
                    .SingleOrDefault(l => l.Email == model.Email);

                if (checkEmailUniqueness != null)
                    ModelState.AddModelError("Email", "Email id is already registered.");

                var checkPhoneUnqiueness = _context.TruckOwners
                    .SingleOrDefault(t => t.Phone == model.Phone);

                if (checkPhoneUnqiueness != null)
                    ModelState.AddModelError("Phone", "Phone number is already registered.");

                var checkTruckUniqueness = _context.Trucks
                    .SingleOrDefault(t => t.LicensePlate == model.LicensePlate);

                if (checkTruckUniqueness != null)
                    ModelState.AddModelError("LicensePlate", "Truck License Number is already registered.");

                var checkDLNumberUniqueness = _context.TruckOwners
                    .SingleOrDefault(t => t.DriverLicenseNumber == model.DriverLicenseNumber);

                if (checkDLNumberUniqueness != null)
                    ModelState.AddModelError("DriverLicenseNumber", "Driver License Number is already registered.");

                var checkVRumberUniqueness = _context.TruckOwners
                    .SingleOrDefault(t => t.VehicleRegNumber == model.VehicleRegNumber);

                if (checkVRumberUniqueness != null)
                    ModelState.AddModelError("VehicleRegNumber", "Vehicle Reg Number is already registered.");

                var checkDIPNumberUniqueness = _context.TruckOwners
                    .SingleOrDefault(t => t.DriverInsurancePolicy == model.DriverInsurancePolicy);

                if (checkDIPNumberUniqueness != null)
                    ModelState.AddModelError("DriverInsurancePolicy", "Driver Insurance Policy is already registered.");
                //Unqieness validations

                if (checkEmailUniqueness == null && checkPhoneUnqiueness == null && checkTruckUniqueness == null && checkDLNumberUniqueness == null && checkVRumberUniqueness == null && checkDIPNumberUniqueness == null)
                {
                    var salt = AuthenticationLogic.Get_SALT(64);

                    var login = new Login
                    {
                        Email = model.Email.Trim(),
                        Password = AuthenticationLogic.Get_HASH_SHA512(model.Password, model.Email, salt),
                        PasswordSalt = salt,
                        UserType = "D",
                        EmailActivated = false,
                        CreatedTime = DateTime.Now,
                        ModifiedTime = DateTime.Now
                    };
                    _context.Logins.Add(login);

                    var truckOwner = new TruckOwner
                    {
                        FirstName = model.FirstName.Trim(),
                        LastName = model.LastName.Trim(),
                        Phone = model.Phone.Trim(),
                        Email = model.Email.Trim(),
                        CurrentStatusActive = false,
                        Address1 = model.Address1.Trim(),
                        Address2 = model.Address2.Trim(),
                        ZipCode = model.ZipCode.Trim(),
                        City = model.City.Trim(),
                        State = model.State.Trim(),
                        DriverLicenseNumber = model.DriverLicenseNumber.Trim(),
                        VehicleRegNumber = model.VehicleRegNumber.Trim(),
                        DriverInsurancePolicy = model.DriverInsurancePolicy.Trim(),
                        CreatedTime = DateTime.Now,
                        ModifiedTime = DateTime.Now
                    };
                    _context.TruckOwners.Add(truckOwner);

                    var truck = new Truck
                    {
                        TruckOwnerId = truckOwner.TruckOwnerId,
                        TruckTypeId = model.TruckTypeId,
                        TruckMake = model.TruckMake.Trim(),
                        TruckModel = model.TruckModel.Trim(),
                        TruckYear = model.TruckYear.ToString(),
                        LicensePlate = model.LicensePlate.Trim(),
                        TruckColor = model.TruckColor.Trim(),
                        CreatedTime = DateTime.Now,
                        ModifiedTime = DateTime.Now
                    };
                    _context.Trucks.Add(truck);
                    _context.SaveChanges();

                    string token = truckOwner.TruckOwnerId + "c45kaa52165hrd84rd";
                    string verificationUrl = Url.Action("VerifyEmail", "TruckOwners", new { token = token }, Request.Url.Scheme);

                    //verificationUrl = "http://189815f4.ngrok.io/TruckOwners/VerifyEmail?token=" + token;

                    SendGridEmailService.SendEmailActivationLink("Driver", truckOwner.Email, truckOwner.FirstName, verificationUrl);

                    TempData["ViewModel"] = new SuccessPageViewModel { Message = Constants.RegisterSuccessMessage };
                    return RedirectToAction("Success", "Home");
                }
            }
            model.TruckTypesList = GetTruckTypes();
            return View("../TruckOwners/BecomeDriver", model);
        }

        [AllowAnonymous]
        public ActionResult VerifyEmail(string token)
        {
            var viewModel = new VerifyEmailViewModel();

            if (String.IsNullOrEmpty(token) || String.IsNullOrWhiteSpace(token))
            {
                viewModel.PageContent = VerifyEmailViewModel.GetInvalidTokenMessage();
                return View(viewModel);
            }

            token = token.Trim();
            string tokenId = token.Substring(0, token.Length - 18);

            if (!Regex.IsMatch(tokenId, @"^\d+$"))
                viewModel.PageContent = VerifyEmailViewModel.GetInvalidTokenMessage();

            int truckOwnerId = Convert.ToInt32(tokenId);

            var truckOwner = _context.TruckOwners.SingleOrDefault(t => t.TruckOwnerId == truckOwnerId);

            if (truckOwner == null)
                viewModel.PageContent = VerifyEmailViewModel.GetInvalidTokenMessage();
            else
            {
                var login = _context.Logins.SingleOrDefault(l => l.Email == truckOwner.Email);
                if (login == null || login.EmailActivated)
                    viewModel.PageContent = VerifyEmailViewModel.GetInvalidTokenMessage();
                else
                {
                    login.EmailActivated = true;
                    _context.SaveChanges();
                    viewModel.PageContent = VerifyEmailViewModel.GetSuccessMessage();
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public ActionResult BecomeDriver()
        {
            var driverRegisterModel = new RegisterDriverViewModel();
            driverRegisterModel.TruckTypesList = GetTruckTypes();
            driverRegisterModel.TruckYear = null;
            return View(driverRegisterModel);
        }

        [CheckDriverAuthorization]
        public ActionResult Trips()
        {
            int truckOwnerId = GetTruckOwnerId();
            IEnumerable<OrderBid> orderBids = _context.OrderBids.Where(o => o.TruckOwnerId == truckOwnerId).ToList();

            ViewBag.Name = GetTruckOwnerName();
            return View(orderBids);
        }

        [CheckDriverAuthorization]
        public ActionResult ShareWithFriend()
        {
            ViewBag.Name = GetTruckOwnerName();
            return View();
        }

        [CheckDriverAuthorization]
        public ActionResult Dashboard()
        {
            string email = GetTruckOwnerEmail();
            var truckOwnerInDb = _context.TruckOwners.Single(t => t.Email == email);
            var truckInDb = _context.Trucks.Single(t => t.TruckOwnerId == truckOwnerInDb.TruckOwnerId);

            var editDriverProfileViewModel = new EditDriverProfileViewModel
            {
                TruckOwnerId = truckOwnerInDb.TruckOwnerId,
                FirstName = truckOwnerInDb.FirstName,
                LastName = truckOwnerInDb.LastName,
                Phone = truckOwnerInDb.Phone,
                Address1 = truckOwnerInDb.Address1,
                Address2 = truckOwnerInDb.Address2,
                ZipCode = truckOwnerInDb.ZipCode,
                City = truckOwnerInDb.City,
                State = truckOwnerInDb.State,
                DriverLicenseNumber = truckOwnerInDb.DriverLicenseNumber,
                VehicleRegNumber = truckOwnerInDb.VehicleRegNumber,
                DriverInsurancePolicy = truckOwnerInDb.DriverInsurancePolicy,
                TruckTypeId = truckInDb.TruckTypeId,
                TruckTypesList = GetTruckTypes(),
                TruckMake = truckInDb.TruckMake,
                TruckModel = truckInDb.TruckModel,
                TruckYear = Convert.ToInt32(truckInDb.TruckYear),
                LicensePlate = truckInDb.LicensePlate,
                TruckColor = truckInDb.TruckColor
            };

            ViewBag.Name = GetTruckOwnerName();
            ViewBag.TruckType = GetTruckTypeName(truckOwnerInDb.TruckOwnerId);
            return View(editDriverProfileViewModel);
        }

        [CheckDriverAuthorization]
        public ActionResult Payment()
        {
            ViewBag.Name = GetTruckOwnerName();
            return View();
        }

        [HttpGet]
        [CheckDriverAuthorization]
        public ActionResult EditProfile()
        {
            int truckOwnerId = GetTruckOwnerId();

            var truckOwnerInDb = _context.TruckOwners.Single(t => t.TruckOwnerId == truckOwnerId);
            var truckInDb = _context.Trucks.Single(t => t.TruckOwnerId == truckOwnerId);

            var editDriverProfileViewModel = new EditDriverProfileViewModel
            {
                TruckOwnerId = truckOwnerInDb.TruckOwnerId,
                FirstName = truckOwnerInDb.FirstName,
                LastName = truckOwnerInDb.LastName,
                Phone = truckOwnerInDb.Phone,
                Address1 = truckOwnerInDb.Address1,
                Address2 = truckOwnerInDb.Address2,
                ZipCode = truckOwnerInDb.ZipCode,
                City = truckOwnerInDb.City,
                State = truckOwnerInDb.State,
                DriverLicenseNumber = truckOwnerInDb.DriverLicenseNumber,
                VehicleRegNumber = truckOwnerInDb.VehicleRegNumber,
                DriverInsurancePolicy = truckOwnerInDb.DriverInsurancePolicy,
                TruckTypeId = truckInDb.TruckTypeId,
                TruckTypesList = GetTruckTypes(),
                TruckMake = truckInDb.TruckMake,
                TruckModel = truckInDb.TruckModel,
                TruckYear = Convert.ToInt32(truckInDb.TruckYear),
                LicensePlate = truckInDb.LicensePlate,
                TruckColor = truckInDb.TruckColor
            };

            ViewBag.Name = GetTruckOwnerName();
            return View(editDriverProfileViewModel);
        }

        [HttpPost]
        [CheckDriverAuthorization]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(EditDriverProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Unqieness validations
                var checkPhoneUnqiueness = _context.TruckOwners
                    .Where(t => t.TruckOwnerId != model.TruckOwnerId)
                    .SingleOrDefault(t => t.Phone == model.Phone);

                if (checkPhoneUnqiueness != null)
                    ModelState.AddModelError("Phone", "Phone number is already registered.");

                var checkTruckUniqueness = _context.Trucks
                    .Where(t => t.TruckOwnerId != model.TruckOwnerId)
                    .SingleOrDefault(t => t.LicensePlate == model.LicensePlate);

                if (checkTruckUniqueness != null)
                    ModelState.AddModelError("LicensePlate", "Truck License Number is already registered.");

                var checkDLNumberUniqueness = _context.TruckOwners
                    .Where(t => t.TruckOwnerId != model.TruckOwnerId)
                    .SingleOrDefault(t => t.DriverLicenseNumber == model.DriverLicenseNumber);

                if (checkDLNumberUniqueness != null)
                    ModelState.AddModelError("DriverLicenseNumber", "Driver License Number is already registered.");

                var checkVRumberUniqueness = _context.TruckOwners
                    .Where(t => t.TruckOwnerId != model.TruckOwnerId)
                    .SingleOrDefault(t => t.VehicleRegNumber == model.VehicleRegNumber);

                if (checkVRumberUniqueness != null)
                    ModelState.AddModelError("VehicleRegNumber", "Vehicle Reg Number is already registered.");

                var checkDIPNumberUniqueness = _context.TruckOwners
                    .Where(t => t.TruckOwnerId != model.TruckOwnerId)
                    .SingleOrDefault(t => t.DriverInsurancePolicy == model.DriverInsurancePolicy);

                if (checkDIPNumberUniqueness != null)
                    ModelState.AddModelError("DriverInsurancePolicy", "Driver Insurance Policy is already registered.");
                //Unqieness validations

                if (checkPhoneUnqiueness == null && checkTruckUniqueness == null && checkDLNumberUniqueness == null && checkVRumberUniqueness == null && checkDIPNumberUniqueness == null)
                {
                    var truckOwnerInDb = _context.TruckOwners.Single(t => t.TruckOwnerId == model.TruckOwnerId);

                    truckOwnerInDb.FirstName = model.FirstName.Trim();
                    truckOwnerInDb.LastName = model.LastName.Trim();
                    truckOwnerInDb.Phone = model.Phone.Trim();
                    truckOwnerInDb.Address1 = model.Address1.Trim();
                    truckOwnerInDb.Address2 = model.Address2.Trim();
                    truckOwnerInDb.ZipCode = model.ZipCode.Trim();
                    truckOwnerInDb.City = model.City.Trim();
                    truckOwnerInDb.State = model.State.Trim();
                    truckOwnerInDb.DriverLicenseNumber = model.DriverLicenseNumber.Trim();
                    truckOwnerInDb.VehicleRegNumber = model.VehicleRegNumber.Trim();
                    truckOwnerInDb.DriverInsurancePolicy = model.DriverInsurancePolicy.Trim();
                    truckOwnerInDb.ModifiedTime = DateTime.Now;

                    var truckInDb = _context.Trucks.Single(t => t.TruckOwnerId == model.TruckOwnerId);

                    truckInDb.TruckTypeId = model.TruckTypeId;
                    truckInDb.TruckMake = model.TruckMake.Trim();
                    truckInDb.TruckModel = model.TruckModel.Trim();
                    truckInDb.TruckYear = model.TruckYear.ToString();
                    truckInDb.LicensePlate = model.LicensePlate.Trim();
                    truckInDb.TruckColor = model.TruckColor.Trim();
                    truckInDb.ModifiedTime = DateTime.Now;

                    _context.SaveChanges();

                    return RedirectToAction("Dashboard", "TruckOwners");
                }
            }
            model.TruckTypesList = GetTruckTypes();
            ViewBag.Name = GetTruckOwnerName();
            return View("../TruckOwners/EditProfile", model);
        }

        private void EnsureLoggedOut()
        {
            // If the request is (still) marked as authenticated we send the user to the logout action  
            if (Request.IsAuthenticated)
                Logout();
        }

        public IEnumerable<TruckType> GetTruckTypes()
        {
            return _context.TruckTypes.ToList();
        }

        private string GetTruckOwnerEmail()
        {
            return this.HttpContext.Session["DriverID"].ToString();
        }

        private int GetTruckOwnerId()
        {
            string email = GetTruckOwnerEmail();
            var truckOwnerId = _context.TruckOwners.Single(t => t.Email == email).TruckOwnerId;
            return truckOwnerId;
        }

        private string GetTruckOwnerName()
        {
            string email = GetTruckOwnerEmail();
            var truckOwner = _context.TruckOwners.Single(t => t.Email == email);
            return truckOwner.FirstName + " " + truckOwner.LastName;
        }

        private string GetTruckTypeName(int truckOwnerId)
        {
            var truckInDb = _context.Trucks.Single(t => t.TruckOwnerId == truckOwnerId);

            return _context.TruckTypes
                .Single(t => t.TruckTypeId == truckInDb.TruckTypeId)
                .Type;
        }
    }
}