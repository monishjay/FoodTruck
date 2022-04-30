//using statements to give access to various libraries and namespaces 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


//these using statements will match your project name 
using Jayakumar_Monish_HW4.DAL;
using Jayakumar_Monish_HW4.Models;

//this namespace will be the same as your project, too 
namespace Jayakumar_Monish_HW4.Controllers
{
    //this is the beginning of the controller class; all controllers inherit from MS’s Controller
    public class ProductsController : Controller
    {
        //this is a private, class-level variable to hold an AppDbContext object so that you are 
        //able to access the database throughout the class 
        private readonly AppDbContext _context;

        //this is a constructor that injects an instance of the AppDbContext into the controller when 
        //the user creates an HTTP request that is routed to an action method on this controller 
        public ProductsController(AppDbContext context)
        {
            //this sets the value of the private variable equal to the AppDbContext object that was  
            //injected into the controller 
            _context = context;
        }
        // GET: Products 
        //This is the action method for the Index page  
        //This will show a list of all the products in the database
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            //retrieve all the products from the database and then  
            //send the user to the Views/Products/Index view  
            return View(await _context.Products.ToListAsync());
        }

        // GET: Products/Details/5 
        //This method will show the details for one product 
        //The user needs to specify which product they want to see 
        //The id parameter represents the ProductID of the product that the user wants to see
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            // Logically, the user MUST specify an id to view.  We can’t show them the details of a       
            // product if we don’t know what product they want to see.  However, the id parameter is 
            // nullable because we would rather show the user an error than have the program crash if  
            // the user did not specify an id.   
            if (id == null) //the user did not specify the id of the product they wanted 
            {
                //The scaffolded code will return a 404 error to the user. 
                //A better line of code here would be the following: 
                //return View("Error", new String[] {"Please specify a product to view! "}); 
                return NotFound();
            }

            //This line of code will look for the desired ProductID in the database 
            //You should change the type of product to product.  The new line would be 
            //Product product = await _context.Products 
            var product = await _context.Products
                .Include(c => c.Suppliers)
                .FirstOrDefaultAsync(c => c.ProductID == id);

            //If the specified id does not match a product in the database, product will be null 
            //Show the user an error message 
            if (product == null)
            {
                //The scaffolded code will return a 404 error to the user. 
                //A better line of code here would be the following: 
                //return View("Error", new String[] {"This product was not found!"}); 
                return NotFound();
            }

            //if code gets this far, everything is okay; send the user to the Product/Details 
            //view with the product you just found in the database 
            return View(product);
        }
        // GET: Products/Create 
        //This method will show the user the blank create product view
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            //Show the user the Views/Product/Create view
            ViewBag.AllSuppliers = GetAllSuppliers();
            return View();
        }

        // POST: Products/Create 
        // To protect from overposting attacks, enable the specific properties you want to bind to, for  
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598. 

        //This tells the second overload of the Create method to respond to HTTP Post requests 
        [HttpPost]

        //This prevents cross-site scripting attacks – this helps make sure that the user actually  
        //intended to make this request.   
        ///You can read more here: https://portswigger.net/web-security/csrf 
        [ValidateAntiForgeryToken]

        //this method signature includes a bind list, which means only the ProductID, Name, Description 
        //and price will be included in this HTTP request. This keeps malicious users from modifying 
        //fields that should not be changed by the user (bank account balance, GPA, etc.)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Description,Price, ProductType")] Product
product, int[] SelectedSuppliers)
        {

            //make sure that the product to be created meets all the rules specified in the  
            //product model class 
            if (ModelState.IsValid)
            {

                //add the product to the database 
                _context.Add(product);

                //save the changes to the database 
                await _context.SaveChangesAsync();

                foreach (int SupplierID in SelectedSuppliers)
                {
                    //find the supplier associated with that id
                    Supplier dbSupplier = _context.Suppliers.Find(SupplierID);

                    //add the supplier to the products's list of suppliers and save changes
                    product.Suppliers.Add(dbSupplier);
                    _context.SaveChanges();
                }

                //send the user back to the Index action to re-generate the list of products 
                return RedirectToAction(nameof(Index));
            }


            //this is the sad path – the model state was not valid, so the user gets sent back to the 
            //create product view to try again.

            ViewBag.AllSuppliers = GetAllSuppliers();

            return View(product);
        }

        private MultiSelectList GetAllSuppliers()
        {

            //Get the list of suppliers from the database
            List<Supplier> SupplierList = _context.Suppliers.ToList();

            //convert the list to a SelectList by calling SelectList constructor
            //SupplierID and SupplierName are the names of the properties on the Supplier class
            //SupplierID is the primary key
            MultiSelectList supplierSelectList = new MultiSelectList(SupplierList.OrderBy(m => m.SupplierID), "SupplierID", "SupplierName");

            //return the MultiSelectList
            return supplierSelectList;
        }

        // GET: Products/Edit/5 
        //This method will allow the user to edit a product 
        //The user needs to specify which product they want to edit 
        //The id parameter represents the ProductID of the product that the user wants to edit
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            // Logically, the user MUST specify an id to edit.  We can’t edit the details of a       
            // product if we don’t know what product they want to edit.  However, the id parameter is 
            // nullable because we would rather show the user an error than have the program crash if  
            // the user did not specify an id.   
            if (id == null)
            {
                //The scaffolded code will return a 404 error to the user. 
                //A better line of code here would be the following: 
                //return View("Error", new String[] {"Please specify a product to edit! "}); 
                return NotFound();
            }

            //This line of code will look for the desired ProductID in the database 
            //You should change the type of product to product.  The new line would be 
            //Product product = await _context.Products.FindAsync(id); 
            var product = await _context.Products.Include(c => c.Suppliers)
                                           .FirstOrDefaultAsync(c => c.ProductID == id);


            //If the specified id does not match a product in the database, product will be null 
            //Show the user an error message 
            if (product == null)
            {
                //The scaffolded code will return a 404 error to the user. 
                //A better line of code here would be the following: 
                //return View("Error", new String[] {"This product was not found!"}); 
                return NotFound();
            }

            //if code gets this far, everything is okay; send the user to the Product/Edit 
            //view with the product you just found in the database

            ViewBag.AllSuppliers = GetAllSuppliers(product);

            return View(product);
        }
        // POST: Products/Edit/5 
        // To protect from overposting attacks, enable the specific properties you want to bind to, for  
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598. 

        //This tells the second overload of the Edit method to respond to HTTP Post requests 
        [HttpPost]

        //This prevents cross-site scripting attacks – this helps make sure that the user actually  
        //intended to make this request.   
        //You can read more here: https://portswigger.net/web-security/csrf 
        [ValidateAntiForgeryToken]

        //this method signature includes a bind list, which means only the ProductID, Name, Description 
        //and price will be included in this HTTP request. This keeps malicious users from modifying 
        //fields that should not be changed by the user (bank account balance, GPA, etc.)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,Name,Description,Price, ProductType")] Product product, int[] SelectedSuppliers)
        {
            //this is a security check to see if the user is trying to modify
            //a different record.  Show an error message
            if (id != product.ProductID)
            {
                return View("Error", new string[] { "Please try again!" });
            }

            if (ModelState.IsValid == false) //there is something wrong
            {
                ViewBag.AllSuppliers = GetAllSuppliers(product);
                return View(product);
            }

            //if code gets this far, attempt to edit the course
            try
            {
                //Find the course to edit in the database and include relevant 
                //navigational properties
                Product dbProduct = _context.Products
                    .Include(c => c.Suppliers)
                    .FirstOrDefault(c => c.ProductID == product.ProductID);

                //create a list of departments that need to be removed
                List<Supplier> SuppliersToRemove = new List<Supplier>();

                //find the departments that should no longer be selected because the
                //user removed them
                //remember, SelectedDepartments = the list from the HTTP request (listbox)
                foreach (Supplier supplier in dbProduct.Suppliers)
                {
                    //see if the new list contains the department id from the old list
                    if (SelectedSuppliers.Contains(supplier.SupplierID) == false)//this department is not on the new list
                    {
                        SuppliersToRemove.Add(supplier);
                    }
                }

                //remove the departments you found in the list above
                //this has to be 2 separate steps because you can't iterate (loop)
                //over a list that you are removing things from
                foreach (Supplier supplier in SuppliersToRemove)
                {
                    //remove this course department from the course's list of departments
                    dbProduct.Suppliers.Remove(supplier);
                    _context.SaveChanges();
                }

                //add the departments that aren't already there
                foreach (int supplierID in SelectedSuppliers)
                {
                    if (dbProduct.Suppliers.Any(d => d.SupplierID == supplierID) == false)//this department is NOT already associated with this course
                    {
                        //Find the associated department in the database
                        Supplier dbSupplier = _context.Suppliers.Find(supplierID);

                        //Add the department to the course's list of departments
                        dbProduct.Suppliers.Add(dbSupplier);
                        _context.SaveChanges();
                    }
                }

                //update the course's scalar properties
                dbProduct.Name = product.Name;
                dbProduct.Description = product.Description;
                dbProduct.Price = product.Price;
                dbProduct.ProductType = product.ProductType;

                //save the changes
                _context.Products.Update(dbProduct);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                return View("Error", new string[] { "There was an error editing this product.", ex.Message });
            }

            //if code gets this far, everything is okay
            //send the user back to the page with all the courses
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Delete/5 
        //This method will allow the user to delete a product 
        //The user needs to specify which product they want to delete 
        //The id parameter represents the ProductID of the product that the user wants to delete
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            // Logically, the user MUST specify an id to delete.  We can’t delete a       
            // product if we don’t know what product they want to delete.  However, the id parameter is 
            // nullable because we would rather show the user an error than have the program crash if  
            // the user did not specify an id.   
            if (id == null)
            {
                //The scaffolded code will return a 404 error to the user. 
                //A better line of code here would be the following: 
                //return View("Error", new String[] {"Please specify a product to delete! "}); 
                return NotFound();
            }
            //This line of code will look for the desired ProductID in the database 
            //You should change the type of product to product.  The new line would be 
            //Product product = await _context.Products 
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductID == id);

            //If the specified id does not match a product in the database, product will be null 
            //Show the user an error message 
            if (product == null)
            {
                //The scaffolded code will return a 404 error to the user. 
                //A better line of code here would be the following: 
                //return View("Error", new String[] {"This product was not found!"}); 
                return NotFound();
            }

            //if code gets this far, everything is okay; send the user to the Product/Delete 
            //view with the product you just found in the database              
            return View(product);
        }

        private MultiSelectList GetAllSuppliers(Product product)
        {
            //Create a new list of departments and get the list of the departments
            //from the database
            List<Supplier> allSuppliers = _context.Suppliers.ToList();

            //loop through the list of course departments to find a list of department ids
            //create a list to store the department ids
            List<Int32> SelectedSuppliersIDs = new List<Int32>();

            //Loop through the list to find the DepartmentIDs
            foreach (Supplier supplier in product.Suppliers)
            {
                SelectedSuppliersIDs.Add(supplier.SupplierID);
            }

            //use the MultiSelectList constructor method to get a new MultiSelectList
            MultiSelectList mslAllSuppliers = new MultiSelectList(allSuppliers.OrderBy(d => d.SupplierName), "SupplierID", "SupplierName", SelectedSuppliersIDs);

            //return the MultiSelectList
            return mslAllSuppliers;
        }
        // POST: Products/Delete/5 
        //This tells this method to respond to HTTP Post requests 
        //It also has to map the Delete action to this method, which is called DeleteConfirmed 
        //This method has to have a different name, because both the GET and POST methods for delete  
        //need a single int parameter, so overloading won’t work.  The ActionName code allows the  
        //routing to still work, even with a different method name 
        [HttpPost, ActionName("Delete")]

        //This prevents cross-site scripting attacks – this helps make sure that the user actually  
        //intended to make this request.   
        //You can read more here: https://portswigger.net/web-security/csrf 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //This line of code will look for the desired ProductID in the database 
            //You should change the type of product to product.  The new line would be 
            //Product product = await _context.Products.FindAsync(id); 
            var product = await _context.Products.FindAsync(id);

            //remove the product from the database 
            _context.Products.Remove(product);

            //save the changes to the database 
            await _context.SaveChangesAsync();

            //Send the user back to back to the Index action to re-generate the list of products 
            return RedirectToAction(nameof(Index));
        }

        //this is a private method to see if a product with a particular id exists in the database 
        //it is used in the Edit/POST action 
        private bool ProductExists(int id)
        {
            //if there is a product with this id in the database, return true 
            //if not, then you return false 
            return _context.Products.Any(e => e.ProductID == id);
        }
    }
}
