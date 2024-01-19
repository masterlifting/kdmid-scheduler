/** @format */

import React from 'react';
import { InputClass } from '../../../styles/input';
import { ButtonClass } from '../../../styles/button';
import { useKdmidIdentifier } from './kdmidIdentifierHooks';

interface IKdmidIdentifierProps {
  chatId: string;
  cityCode: string;
}

export const KdmidIdentifier = ({ chatId, cityCode }: IKdmidIdentifierProps) => {
  const { userKdmidIds, onSubmit, onChangeId, onChangeCd, onChangeEms, onRemove, onAdd } = useKdmidIdentifier(chatId, cityCode);

  return (
    <div className='w-80 absolute rounded-md left-1/2 top-1/3 transform -translate-x-1/2 -translate-y-1/3'>
      <h1 className='text-2xl font-bold mb-2'>Kdmid Identifiers</h1>
      <div className=' h-80 overflow-y-auto'>
        {userKdmidIds.map(x => (
          <form key={x.command.id} onSubmit={e => onSubmit(e, x.command.id)} className='mt-3'>
            <span className='text-l font-bold mb-2'>{x.city}</span>
            <div className='flex flex-col items-center'>
              <input
                className={InputClass.Text}
                name='id'
                title='id'
                type='text'
                placeholder='id'
                autoComplete='id'
                value={x.identifier.id}
                onChange={e => onChangeId(e, x.command.id)}
              />
              <input
                className={InputClass.Text}
                name='cd'
                title='cd'
                type='text'
                placeholder='cd'
                autoComplete='cd'
                value={x.identifier.cd}
                onChange={e => onChangeCd(e, x.command.id)}
              />
              <input
                className={InputClass.Text}
                name='ems'
                title='ems'
                type='text'
                placeholder='ems'
                autoComplete='ems'
                value={x.identifier.ems}
                onChange={e => onChangeEms(e, x.command.id)}
              />
            </div>
            <div
              className='
              grid grid-cols-2 gap-1
            '
            >
              <div
                className='
              grid grid-cols-2 gap-1
              '
              >
                <button type='button' className={ButtonClass.Danger} onClick={e => onRemove(e, x.command.id)}>
                  -
                </button>
                <button type='button' className={ButtonClass.Success} onClick={e => onAdd(e, x.command.id)}>
                  +
                </button>
              </div>
              <button type='submit' className={ButtonClass.Success}>
                Submit
              </button>
            </div>
          </form>
        ))}
      </div>
    </div>
  );
};
