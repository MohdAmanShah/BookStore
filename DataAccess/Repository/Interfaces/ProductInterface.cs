﻿using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Interfaces
{
    public interface ProductInterface : RepositoryInterface<ProductModel>
    {
        void Update(ProductModel Product); 
    }
}
