using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Jayakumar_Monish_HW4.DAL;
using Jayakumar_Monish_HW4.Models;

namespace Jayakumar_Monish_HW4.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly AppDbContext _context;

        public OrderDetailsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OrderDetails
        public async Task<IActionResult> Index(int? orderID)
        {

            if (orderID == null)
            {
                return View("Error", new String[] { "Please specify an order to view!" });
            }

            //limit the list to only the registration details that belong to this registration
            List<OrderDetail> ods = _context.OrderDetails
                                          .Include(rd => rd.DetailProduct)
                                          .Where(rd => rd.DetailOrder.OrderID == orderID)
                                          .ToList();

            return View(ods);
        }


        /*
        // GET: OrderDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .FirstOrDefaultAsync(m => m.OrderDetailID == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        */

        // GET: OrderDetails/Create
        public IActionResult Create(int OrderID)
        {
            //create a new instance of the orderdetail class
            OrderDetail od = new OrderDetail();

            //find the order that should be associated with this order
            Order dbOrder = _context.Orders.Find(OrderID);

            //set the new order detail's order equal to the order you just found
            od.DetailOrder = dbOrder;

            //populate the ViewBag with a list of existing products
            ViewBag.AllProducts = GetAllProducts();

            return View(od);
        }

        // POST: OrderDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken] // TODO: not sure if i should include OrderDetailID to be binded
        public async Task<IActionResult> Create(OrderDetail orderDetail, int selectedProduct)
        {


            //if user has not entered all fields, send them back to try again
            if (ModelState.IsValid == false)
            {
                ViewBag.AllProducts = GetAllProducts();
                return View(orderDetail);
            }

            //find the product to be associated with this order
            Product dbProduct = _context.Products.Find(selectedProduct);

            //set the order detail's product to be equal to the one we just found
            orderDetail.DetailProduct = dbProduct;

            //find the registration on the database that has the correct registration id
            //unfortunately, the HTTP request will not contain the entire registration object, 
            //just the registration id, so we have to find the actual object in the database
            Order dbOrder = _context.Orders.Find(orderDetail.DetailOrder.OrderID);

            //set the registration on the registration detail equal to the registration that we just found
            orderDetail.DetailOrder = dbOrder;

            //set the order detail's price equal to the course price
            //this will allow us to to store the price that the user paid
            //orderDetail.DetailProduct.Name = dbProduct.Name;
            orderDetail.ProductPrice = dbProduct.Price;

            //calculate the extended price for the registration detail
            orderDetail.ExtendedPrice = orderDetail.Quantity * orderDetail.ProductPrice;

            //add the registration detail to the database
            _context.Add(orderDetail);
            await _context.SaveChangesAsync();

            //send the user to the details page for this registration
            return RedirectToAction("Details", "Orders", new { id = orderDetail.DetailOrder.OrderID });
        }

        // GET: OrderDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //find the registration detail
            OrderDetail orderDetail = await _context.OrderDetails
                                                   .Include(rd => rd.DetailProduct)
                                                   .Include(rd => rd.DetailOrder)
                                                   .FirstOrDefaultAsync(rd => rd.OrderDetailID == id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            return View(orderDetail);
        }

        // POST: OrderDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderDetailID,Quantity")] OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID)
            {
                return NotFound();
            }

            //information is not valid, try again
            if (ModelState.IsValid == false)
            {
                return View(orderDetail);
            }

            //create a new registration detail
            OrderDetail dbOD;
            //if code gets this far, update the record
            try
            {
                //find the existing registration detail in the database
                //include both registration and course
                dbOD = _context.OrderDetails
                      .Include(rd => rd.DetailProduct)
                      .Include(rd => rd.DetailOrder)
                      .FirstOrDefault(rd => rd.OrderDetailID == orderDetail.OrderDetailID);

                //update the scalar properties
                dbOD.Quantity = orderDetail.Quantity;
                dbOD.ProductPrice = dbOD.DetailProduct.Price;
                dbOD.ExtendedPrice = dbOD.Quantity * dbOD.ProductPrice;

                //save changes
                _context.Update(dbOD);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return View("Error", new String[] { "There was a problem editing this record", ex.Message });
            }

            //if code gets this far, go back to the registration details index page
            return RedirectToAction("Details", "Orders", new { id = dbOD.DetailOrder.OrderID });
        }

        // GET: OrderDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .FirstOrDefaultAsync(m => m.OrderDetailID == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Orders", new { id = orderDetail.DetailOrder.OrderID });
        }

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails.Any(e => e.OrderDetailID == id);
        }

        private SelectList GetAllProducts()
        {
            //create a list for all the courses
            List<Product> allProducts = _context.Products.ToList();

            //the user MUST select a course, so you don't need a dummy option for no course

            //use the constructor on select list to create a new select list with the options
            SelectList slAllProducts = new SelectList(allProducts, nameof(Product.ProductID), nameof(Product.Name));

            return slAllProducts;
        }
    }
}
