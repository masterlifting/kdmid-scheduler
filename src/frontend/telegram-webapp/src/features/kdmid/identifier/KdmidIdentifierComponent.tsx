/** @format */

import React from 'react';
import { InputClass } from '../../../styles/input';
import { ButtonClass } from '../../../styles/button';
import { useKdmidIdentifier } from './kdmidIdentifierHooks';
import { Paginator } from '../../../components/PaginationComponents';

interface IKdmidIdentifierProps {
  chatId: string;
  cityCode: string;
}

export const KdmidIdentifier = ({ chatId, cityCode }: IKdmidIdentifierProps) => {
  const {
    city,
    commands,
    commandsTotalCount,
    onNewCommand,
    onSetCommand,
    onRemoveCommand,
    onChangeKdmidId,
    onChangeKdmidCd,
    onChangeKdmidEms,
    paginationState,
    setPaginstionState,
  } = useKdmidIdentifier(chatId, cityCode);

  return (
    <div className='w-80 absolute rounded-md left-1/2 top-1/3 transform -translate-x-1/2 -translate-y-1/3'>
      <div className='grid grid-cols-[1fr,auto] gap-1 items-center'>
        <span className='text-3xl font-bold text-left'>{city?.name ?? 'Loading...'}</span>
        <button type='button' className={ButtonClass.Success} onClick={onNewCommand}>
          New
        </button>
      </div>
      <div>
        {commands.map(x => (
          <form key={x.key} className='mt-3'>
            <div className='flex flex-col items-center'>
              <input
                className={InputClass.Text}
                name='id'
                title='id from your kdmid email link'
                type='text'
                placeholder='id from your link'
                autoComplete='id'
                value={x.command.identifier.id}
                onChange={e => onChangeKdmidId(e, x.key)}
              />
              <input
                className={InputClass.Text}
                name='cd'
                title='cd from your kdmid email link'
                type='text'
                placeholder='cd from your link'
                autoComplete='cd'
                value={x.command.identifier.cd}
                onChange={e => onChangeKdmidCd(e, x.key)}
              />
              <input
                className={InputClass.Text}
                name='ems'
                title='ems from your kdmid email link. Optional'
                type='text'
                placeholder='ems from your link'
                autoComplete='ems'
                value={x.command.identifier.ems}
                onChange={e => onChangeKdmidEms(e, x.key)}
              />
            </div>
            <div className='grid grid-cols-2 gap-1'>
              <button type='button' className={ButtonClass.Danger} onClick={e => onRemoveCommand(e, x.key)}>
                Remove
              </button>
              <button
                type='button'
                className={x.command.id ? ButtonClass.Primary : ButtonClass.Success}
                onClick={e => onSetCommand(e, x.key)}
              >
                {x.command.id ? 'Update' : 'Create'}
              </button>
            </div>
          </form>
        ))}
        <Paginator totalItemsCount={commandsTotalCount} state={paginationState} setPaginatonState={setPaginstionState} />
      </div>
    </div>
  );
};
