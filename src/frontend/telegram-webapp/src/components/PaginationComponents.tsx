/** @format */

import React from 'react';
import { IPagination } from '../_types';
import { HoveredTextColor } from '../styles/colors';
import { ButtonClass } from '../styles/button';

interface IPaginatorProps {
  totalItemsCount: number;
  state: IPagination;
  setPaginatonState: (configuration: IPagination) => void;
}

export const Paginator = ({ totalItemsCount, state, setPaginatonState }: IPaginatorProps) => {
  return (
    <div className='flex justify-between items-center mt-5'>
      <span>
        {state.pageNumber !== 1 && (
          <button
            type='button'
            className={ButtonClass.Secondary}
            onClick={_ => setPaginatonState({ ...state, pageNumber: state.pageNumber - 1 })}
          >
            {'<<<'}
          </button>
        )}
      </span>

      <span>
        {totalItemsCount > state.pageNumber && (
          <button
            type='button'
            className={ButtonClass.Secondary}
            onClick={_ => setPaginatonState({ ...state, pageNumber: state.pageNumber + 1 })}
          >
            {'>>>'}
          </button>
        )}
      </span>
    </div>
  );
};
