using System;
using System.DirectoryServices;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.IO;
using FileIO = System.IO.File;
using Flashcards.ContentManager.Models;
using System.Configuration;
using System.Web.Security;

namespace Flashcards.ContentManager.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var baseDir = Server.MapPath("~/App_Data");
            var directories = Directory.EnumerateDirectories(baseDir);
            var contents = new List<ContentModel>();

            foreach (string directory in directories)
            {
                var dirInfo = new DirectoryInfo(directory);
                contents.Add(new ContentModel
                {
                    Name = dirInfo.Name,
                    DateUploaded = dirInfo.LastWriteTime
                });
            }

            return View(contents);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var isValidUser = AuthenticateUser(model);
                if (isValidUser)
                {
                    SetAuthorizationCookie(model);
                    return LoginSuccess(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Sorry, your username and password are incorrect. Please try again.");
                }
            }
            else
                ModelState.AddModelError("", "Sorry, your username and password are incorrect. Please try again.");

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.PageTitle = "Log In";

            return View(model);
        }

        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase packageFile)
        {
            if (packageFile != null)
            {
                //Extract File
                string baseDirectoryName = Path.Combine(Server.MapPath("~/App_Data"),Path.GetFileNameWithoutExtension(packageFile.FileName));
                if (!Directory.Exists(baseDirectoryName))
                    Directory.CreateDirectory(baseDirectoryName);

                string destFilePath = Path.Combine(baseDirectoryName, packageFile.FileName);

                packageFile.SaveAs(destFilePath);

                if (FileIO.Exists(destFilePath))
                {
                    using (ZipInputStream s = new ZipInputStream(FileIO.OpenRead(destFilePath)))
                    {
                        ZipEntry theEntry;
                        while ((theEntry = s.GetNextEntry()) != null)
                        {
                            string directoryName = Path.GetDirectoryName(theEntry.Name);
                            string fileName = Path.Combine(baseDirectoryName, Path.GetFileName(theEntry.Name));

                            // create directory
                            if (directoryName.Length > 0)
                            {
                                Directory.CreateDirectory(directoryName);
                            }

                            if (fileName != String.Empty)
                            {
                                using (FileStream streamWriter = FileIO.Create(fileName))
                                {

                                    int size = 2048;
                                    byte[] data = new byte[2048];
                                    while (true)
                                    {
                                        size = s.Read(data, 0, data.Length);
                                        if (size > 0)
                                        {
                                            streamWriter.Write(data, 0, size);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Please specify file to be uploaded.");

            return View("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private static bool AuthenticateUser(LoginModel model)
        {
            var directoryPath = ConfigurationManager.AppSettings["ldapAuthPath"];
            var isAuthenticated = false;

            try
            {
                var directoryEntry = new DirectoryEntry(directoryPath, model.UserName, model.Password);
                var nativeObj = directoryEntry.NativeObject;
                isAuthenticated = true;
            }
            catch (Exception e)
            {
                isAuthenticated = false;
            }

            return isAuthenticated;
        }

        private void SetAuthorizationCookie(LoginModel model)
        {
            var authTicket = new FormsAuthenticationTicket(model.UserName, false, 30);
            var hashedTicket = FormsAuthentication.Encrypt(authTicket);
            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, hashedTicket);
            Response.Cookies.Add(authCookie);

            authCookie = new HttpCookie("accept", "true");
            authCookie.Expires = DateTime.Now.AddMinutes(30);
            Response.Cookies.Add(authCookie);
        }

        private ActionResult LoginSuccess(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 &&
                    returnUrl.StartsWith("/") &&
                    !returnUrl.StartsWith("//") &&
                    !returnUrl.StartsWith("/\\"))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
