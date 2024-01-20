/** @format */

import React from 'react';
import { Route, Routes } from 'react-router-dom';
import { KdmidIdentifierPage } from './pages/KdmidIdentifierPage';

export const App = () => (
  <div className='container mx-auto max-w-2xl pt-5'>
    <Routes>
      <Route path='/kdmidid' element={<KdmidIdentifierPage />} />
      <Route path='*' element={<h1>This page does not exist</h1>} />
    </Routes>
  </div>
);
