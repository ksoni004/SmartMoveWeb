//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SmartMoveWebApp
{
    using System;
    using System.Collections.Generic;
    
    public partial class CustomerCreditCard
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CustomerCreditCard()
        {
            this.OrderPayments = new HashSet<OrderPayment>();
        }
    
        public int CCId { get; set; }
        public int CustomerId { get; set; }
        public string CCNumber { get; set; }
        public string CCName { get; set; }
        public string CCType { get; set; }
        public int CCCRVNumber { get; set; }
        public int CCExpiryMonth { get; set; }
        public int CCExpiryYear { get; set; }
        public string CCZipCode { get; set; }
        public System.DateTime CreatedTime { get; set; }
    
        public virtual Customer Customer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderPayment> OrderPayments { get; set; }
    }
}