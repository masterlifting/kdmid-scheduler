/** @format */

import React from 'react';
import { Route, Routes } from 'react-router-dom';
import { KdmidIdentifierPage } from './pages/KdmidIdentifierPage';

export const App = () => (
  <div className='container mx-auto w-80 max-w-80 pt-5'>
    <Routes>
      <Route path='/kdmidId' element={<KdmidIdentifierPage />} />
      <Route path='*' element={<h1>This page does not exist</h1>} />
    </Routes>
  </div>
);
