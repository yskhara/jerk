#pragma once

// ジャーク制約による台形加速度プロファイル
// 変位は時間に関する3次関数で表現される
class MotionBlock
{
	// 各項の係数
	double k[4];

	// ramaining time-step to be "segmented out."
	// 或いはどこまで完了したかを保持してもよいだろう
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
