/** @format */

import React, { useState } from 'react';
import { InputClass } from '../../../styles/input';
import { ButtonClass } from '../../../styles/button';
import { useKdmidIdentifier } from './kdmidIdentifierHooks';
import { Paginator } from '../../../components/PaginationComponents';
import { TextColor } from '../../../styles/colors';

interface IKdmidIdentifierProps {
  chatId: string;
  cityCode: string;
}

export const KdmidIdentifier = ({ chatId, cityCode }: IKdmidIdentifierProps) => {
  const [isVisibleInstruction, setIsVisibleInstruction] = useState(false);
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
    <div>
      <div className='grid grid-cols-[1fr,0.5fr] gap-1 items-center mb-1'>
        <span className='text-3xl font-bold text-left'>{city?.name ?? 'Loading...'}</span>
        <button type='button' className={ButtonClass.Success} onClick={onNewCommand}>
          New
        </button>
      </div>
      <b
        className={`${!isVisibleInstruction && TextColor.Secondary} cursor-pointer hover:underline`}
        onClick={() => setIsVisibleInstruction(!isVisibleInstruction)}
      >
        {isVisibleInstruction ? 'Hide' : 'Show'} instructions
      </b>
      <div className={`max-h-0 overflow-hidden transition-all duration-500 ${isVisibleInstruction ? 'max-h-[1000px]' : ''}`}>
        <span className={`text-sm ${TextColor.Secondary}`}>
          <br />
          1. Register your service on the kdmid.ru portal
          <br />
          2. Activate the received link from your email
          <br />
          3. Extract values of ID, CD, and EMS from the link
          <br />
          4. Press the 'New' button
          <br />
          5. Fill out the form below
          <br />
          6. Press the 'Create' button
          <br />
          Your appointment will be checked every 25 minutes, starting from the time you created the command until the working time
          of your embassy, and is not more than 23 times per day.
        </span>
      </div>
      <div>
        {commands.map(x => (
          <form key={x.key} className='mt-1'>
            <div className='flex flex-col items-center'>
              <input
                className={InputClass.Text}
                name='id'
                title='id from your kdmid email link'
                type='text'
                placeholder='ID from your email link'
                autoComplete='id'
                value={x.command.identifier.id}
                onChange={e => onChangeKdmidId(e, x.key)}
              />
              <input
                className={InputClass.Text}
                name='cd'
                title='cd from your kdmid email link'
                type='text'
                placeholder='CD from your email link'
                autoComplete='cd'
                value={x.command.identifier.cd}
                onChange={e => onChangeKdmidCd(e, x.key)}
              />
              <input
                className={InputClass.Text}
                name='ems'
                title='ems from your kdmid email link. Optional'
                type='text'
                placeholder='EMS from your email link'
                autoComplete='ems'
                value={x.command.identifier.ems}
                onChange={e => onChangeKdmidEms(e, x.key)}
              />
            </div>
            <div className='grid grid-cols-2 gap-3'>
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
