/** @format */

import React from 'react';
import { IPagination } from '../_types';
import { HoveredTextColor } from '../styles/colors';

interface IPaginatorProps {
  totalItemsCount: number;
  state: IPagination;
  setPaginatonState: (configuration: IPagination) => void;
}

export const Paginator = ({ totalItemsCount, state, setPaginatonState }: IPaginatorProps) => {
  return (
    <div className='flex justify-between items-center mt-2 p-3'>
      <div>
        {state.pageNumber !== 1 && (
          <span
            className={HoveredTextColor.Primary}
            onClick={_ => setPaginatonState({ ...state, pageNumber: state.pageNumber - 1 })}
          >
            <svg width='50px' height='50px' viewBox='0 0 117 117'>
              <g fill='none' fill-rule='evenodd' stroke-width='1'>
                <g fill-rule='nonzero' id='left-arrow'>
                  <path
                    d='M58.5,116.6 C90.5,116.6 116.6,90.6 116.6,58.5 C116.6,26.4 90.6,0.5 58.5,0.5 C26.4,0.5 0.5,26.5 0.5,58.5 C0.5,90.5 26.5,116.6 58.5,116.6 Z M58.5,8.6 C86,8.6 108.4,31 108.4,58.5 C108.4,86 86,108.4 58.5,108.4 C31,108.4 8.6,86 8.6,58.5 C8.6,31 31,8.6 58.5,8.6 Z'
                    fill='#4A4A4A'
                  />
                  <path
                    d='M64,87.5 C64.8,88.3 65.8,88.7 66.9,88.7 C67.9,88.7 69,88.3 69.8,87.5 C71.4,85.9 71.4,83.3 69.8,81.7 L46.3,58.2 L69.8,34.7 C71.4,33.1 71.4,30.5 69.8,28.9 C68.2,27.3 65.6,27.3 64,28.9 L37.6,55.3 C36.8,56.1 36.4,57.1 36.4,58.2 C36.4,59.3 36.8,60.3 37.6,61.1 L64,87.5 Z'
                    fill='#17AB13'
                  />
                </g>
              </g>
            </svg>
          </span>
        )}
      </div>

      <div>
        {totalItemsCount > state.pageNumber && (
          <span
            className={HoveredTextColor.Primary}
            onClick={_ => setPaginatonState({ ...state, pageNumber: state.pageNumber + 1 })}
          >
            <svg width='50px' height='50px' viewBox='0 0 117 117' className={HoveredTextColor.Primary}>
              <g fill='none' fill-rule='evenodd' stroke='none' stroke-width='1'>
                <g fill-rule='nonzero' id='right-arrow'>
                  <path
                    d='M58.5,116.6 C90.5,116.6 116.6,90.6 116.6,58.5 C116.6,26.4 90.5,0.4 58.5,0.4 C26.5,0.4 0.4,26.5 0.4,58.5 C0.4,90.5 26.5,116.6 58.5,116.6 Z M58.5,8.6 C86,8.6 108.4,31 108.4,58.5 C108.4,86 86,108.4 58.5,108.4 C31,108.4 8.6,86 8.6,58.5 C8.6,31 31,8.6 58.5,8.6 Z'
                    fill='#4A4A4A'
                  />
                  <path
                    d='M45.1,87.5 C45.9,88.3 46.9,88.7 48,88.7 C49,88.7 50.1,88.3 50.9,87.5 L77.3,61.1 C78.1,60.3 78.5,59.3 78.5,58.2 C78.5,57.1 78.1,56.1 77.3,55.3 L50.9,28.9 C49.3,27.3 46.7,27.3 45.1,28.9 C43.5,30.5 43.5,33.1 45.1,34.7 L68.6,58.2 L45.1,81.7 C43.5,83.3 43.5,85.9 45.1,87.5 Z'
                    fill='#17AB13'
                  />
                </g>
              </g>
            </svg>
          </span>
        )}
      </div>
    </div>
  );
};
