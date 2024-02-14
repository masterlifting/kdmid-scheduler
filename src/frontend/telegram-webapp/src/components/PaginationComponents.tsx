/** @format */

import React from 'react';
import { IPagination } from '../_types';
import { ButtonClass } from '../styles/button';

interface IPaginatorProps {
  totalItemsCount: number;
  state: IPagination;
  setPaginatonState: (configuration: IPagination) => void;
}

export const Paginator = ({ totalItemsCount, state, setPaginatonState }: IPaginatorProps) => {
  return (
    <div className='mt-1 grid grid-cols-2'>
      <div className='w-14'>
        {state.pageNumber !== 1 && (
          <button
            type='button'
            className={ButtonClass.Secondary}
            onClick={_ => setPaginatonState({ ...state, pageNumber: state.pageNumber - 1 })}
          >
            {'<'}
          </button>
        )}
      </div>

      <div className='w-14 ml-auto'>
        {totalItemsCount > state.pageNumber && (
          <button
            type='button'
            className={ButtonClass.Secondary}
            onClick={_ => setPaginatonState({ ...state, pageNumber: state.pageNumber + 1 })}
          >
            {'>'}
          </button>
        )}
      </div>
    </div>
  );
};
