using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : Controller
    {
        private readonly ContactsAPIDbContext _dbContext;

        public ContactsController(ContactsAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        [HttpGet]
        public async Task<IActionResult> GetContacts()
        {
            return Ok(await _dbContext.Contacts.ToListAsync());
        }
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetContactById([FromRoute] Guid id)
        {
            var contact = await _dbContext.Contacts.FindAsync(id);
            if (contact != null)
            {
                return Ok(contact);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> AddContact(AddContacts addContacts)
        {
            var contact = new Contacts()
            {
                Id = Guid.NewGuid(),
                Name = addContacts.Name,
                Email = addContacts.Email,
                Address = addContacts.Address,
                Phone = addContacts.Phone
            };
            await _dbContext.Contacts.AddAsync(contact);
            await _dbContext.SaveChangesAsync();

            return Ok(contact);

        }
        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> UpdateContact([FromRoute] Guid id, UpdateContacts updatedContact)
        {
            var contacts = await _dbContext.Contacts.FindAsync(id);
            if (contacts != null)
            {
                contacts.Name = updatedContact.Name;
                contacts.Email = updatedContact.Email;
                contacts.Address = updatedContact.Address;
                contacts.Phone = updatedContact.Phone;

                await _dbContext.SaveChangesAsync();

                return Ok(contacts);
            }

            return NotFound();

        }

        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> DeleteContact([FromRoute] Guid id)
        {
            var contact = await _dbContext.Contacts.FindAsync(id);
            if (contact != null)
            {
                _dbContext.Remove(contact);
                await _dbContext.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

    }
}