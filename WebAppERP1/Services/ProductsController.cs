using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using WebAppERP1.Models.Dtos;

namespace WebAppERP1.Services
{
    public class ProductsController : ApiController
    {
        SqlConnection _conn;
        SqlCommand _cmd;
        SqlDataAdapter _da;
        DataTable _dt;

        string query;
        int result;


        public ProductsController()
        {
            _conn = new SqlConnection();
            _cmd = new SqlCommand();
            _conn.ConnectionString = "Data Source=.\\SQLEXPRESS;database=myFirstCruddb;trusted_connection=true;";
            _da = new SqlDataAdapter();
            _dt = new DataTable();
        }
           
        [HttpGet]
        [Route("api/ProductsWS")]
        public async Task<ProductDto> GetProductsWS()
        {
            ProductDto resp = null;

            var uri ="http://cloudonapi.oncloud.gr/s1services/JS/updateItems/cloudOnTest";

            var res = await GetAsync(uri);
            //var serializerSettings = new JsonSerializerSettings();
            //serializerSettings.Culture = new CultureInfo("el-GR", false);
            resp = JsonConvert.DeserializeObject<ProductDto>(res/*, serializerSettings*/);

            var result = new ProductDto
            {
                Success = resp.Success,
                Data = resp.Data.Take(10).ToList()
            };


            return result;
               
        }

        public async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        // GET api/<controller>
        public List<Product> Get()
        {
            List<Product> products = new List<Product>();
            try
            {
                //create a query for retrieving data in the database.
                query = "SELECT Id, ExternalId, Code, Description, [Name], Barcode, RetailPrice, WholesalePrice, Discount FROM PRODUCTS";
                
                _cmd.Connection = _conn;
                _cmd.CommandText = query;
                _da.SelectCommand = _cmd;
                _da.Fill(_dt);

                for (int i = 0; i < _dt.Rows.Count; i++)
                {
                    products.Add(
                        new Product{
                        Id = _dt.Rows[i].Field<long>("Id"),
                        ExternalId = _dt.Rows[i].Field<string>("ExternalId"),
                        Code = _dt.Rows[i].Field<string>("Code"),
                        Description = _dt.Rows[i].Field<string>("Description"),
                        Name = _dt.Rows[i].Field<string>("Name"),
                        Barcode = _dt.Rows[i].Field<string>("Barcode"),
                        RetailPrice = _dt.Rows[i].Field<string>("RetailPrice"),
                        WholesalePrice = _dt.Rows[i].Field<string>("WholesalePrice"),
                        Discount = _dt.Rows[i].Field<string>("Discount"),
                        });
                }
                //set the data that to be display in the datagridview
                //--jqueryDataTable.DataSource = _dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _da.Dispose();
                _conn.Close();

            }

            return products;
        }

        // GET api/<controller>/5
        public Product Get(int id)
        {
            return new Product();
        }

        // POST api/<controller>
        public void Post([FromBody]List<Product> products)
        {            
            SqlTransaction _transaction = null;

            try
            {
               
                foreach (var product in products)
                {

                    _cmd.Parameters.AddWithValue("@ExternalId", product.ExternalId);
                    _cmd.Parameters.AddWithValue("@Code", product.Code);
                    _cmd.Parameters.AddWithValue("@Description", product.Description);
                    _cmd.Parameters.AddWithValue("@Name", product.Name);
                    _cmd.Parameters.AddWithValue("@Barcode", product.Barcode);
                    _cmd.Parameters.AddWithValue("@RetailPrice", product.RetailPrice);
                    _cmd.Parameters.AddWithValue("@WholesalePrice", product.WholesalePrice);
                    _cmd.Parameters.AddWithValue("@Discount", product.Discount);

                    query = query + "INSERT INTO Products (" +
                        "ExternalId," +
                        "Code,"+
                        "Description,"+
                        "Name,"+
                        "Barcode,"+
                        "RetailPrice,"+
                        "WholesalePrice,"+
                        "Discount"+
                        ") VALUES('" +
                        "@ExternalId, " +
                        "@Code, " + 
                        "@Description, " + 
                        "@Name, "  + 
                        "@Barcode, " + 
                        "@RetailPrice, " + 
                        "@WholesalePrice, " + 
                        "@Discount" + 
                        "'); ";                  
                } // End ForEach

                _conn.Open();                

                // Start a local transaction.
                _transaction = _conn.BeginTransaction("Multi-ProductsTransaction");
                _cmd.Connection = _conn;
                _cmd.CommandText = query;
                //execute the data.
                result = _cmd.ExecuteNonQuery();

                _transaction.Commit();
                Console.WriteLine("Data has been saved in the SQL database"); 
               
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);

                try
                {
                    _transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    // This catch block will handle any errors that may have occurred 
                    // on the server that would cause the rollback to fail, such as 
                    // a closed connection.
                    Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                    Console.WriteLine("  Message: {0}", ex2.Message);
                }
            }
            finally
            {
                _conn.Close();
            } 

        }

        // PUT api/<controller>/5
        public void Put(long id, [FromBody]Product product)
        {
            try
            {
                _conn.Open();

                query = "UPDATE Products SET EXTERNALID ='" +
                    product.ExternalId +
                    "', CODE ='" + product.Code +
                    "', DESCRIPTION ='" + product.Description +
                    "', NAME ='" + product.Name +
                    "', BARCODE ='" + product.Barcode +
                    "', RETAILPRICE ='" + product.RetailPrice +
                    "', WHOLESALEPRICE ='" + product.WholesalePrice +
                    "', DISCOUNT ='" + product.Discount +
                    "' WHERE ID = " + id;

                _cmd.Connection = _conn;
                _cmd.CommandText = query;
                result = _cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    Console.WriteLine("Data has been updated in the SQL database");
                }
                else
                {
                    Console.WriteLine("SQL QUERY ERROR");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _conn.Close();
            }
        }

        // DELETE api/<controller>/5
        public void Delete(long id)
        {
            try
            {

                _conn.Open();
                //delete query
                query = "DELETE FROM PRODUCTS WHERE ID = " + id;
                //it holds the data to be executed.
                _cmd.Connection = _conn;
                _cmd.CommandText = query;
                //execute the data.
                result = _cmd.ExecuteNonQuery();
                //validate the result of the executed query.
                if (result > 0)
                {
                    Console.WriteLine("Data has been deleted in the SQL database");
                }
                else
                {
                    Console.WriteLine("SQL QUERY ERROR");
                }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _conn.Close();
            }
        }
    }
}