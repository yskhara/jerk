#pragma once

// �W���[�N����ɂ���`�����x�v���t�@�C��
// �ψʂ͎��ԂɊւ���3���֐��ŕ\�������
class MotionBlock
{
	// �e���̌W��
	double k[4];

	// ramaining time-step to be "segmented out."
	// �����͂ǂ��܂Ŋ�����������ێ����Ă��悢���낤
	int remaining_timestep;

	double exit_position;
	double exit_velocity;
	double exit_acceleration;
};

class MotionController
{
public:
	MotionController(void)
	{
		block_buffer = new MotionBlock[block_buffer_size];


	}

	// set next target ?


	void update_plan(void)
	{
		// recalculate?
		// calculate segments and fill the segment buffer
	}

private:
	MotionBlock *block_buffer;
	int block_buffer_head;
	int block_buffer_tail;
	static constexpr int block_buffer_size = 16;
	static constexpr int block_buffer_mask = 15;



};
